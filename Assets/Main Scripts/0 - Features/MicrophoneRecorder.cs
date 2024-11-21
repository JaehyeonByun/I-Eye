using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.IO;

public class MicrophoneRecorder : MonoBehaviour
{
    [SerializeField]
    private string apiKeyId = "YOUR_API_KEY_ID"; // Naver API Key ID
    [SerializeField]
    private string apiKeySecret = "YOUR_API_KEY_SECRET"; // Naver API Key Secret
    [SerializeField]
    private string apiUrl = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor"; // Naver STT API URL
    [SerializeField]
    private TextMeshProUGUI resultText; // 음성 인식 결과 표시용 TextMeshProUGUI

    private string microphoneID = null; // 사용할 마이크 ID
    private AudioClip recording = null; // 녹음된 오디오 클립
    private int recordingLengthSec = 30; // 녹음 시간 (초)
    private int recordingHZ = 16000;    // 샘플링 레이트

    private bool isForPitchAnalysis = false; // 피치 분석 플래그

    public bool IsRecognitionComplete { get; private set; } = false; // 음성 인식 완료 상태 플래그
    public bool IsPitchAnalyzeComplete { get; private set; } = false;

    private void Start()
    {
        // 마이크 확인 및 초기화
        if (Microphone.devices.Length > 0)
        {
            microphoneID = Microphone.devices[0];
            Debug.Log($"[MicrophoneRecorder] Detected Microphone: {microphoneID}");
        }
        else
        {
            Debug.LogError("[MicrophoneRecorder] No microphone detected!");
        }
    }

    /// <summary>
    /// 녹음 시작 (음성 인식 또는 피치 분석 구분)
    /// </summary>
    /// <param name="forPitchAnalysis">피치 분석용인지 여부</param>
    public void StartRecording(bool forPitchAnalysis = false)
    {
        if (string.IsNullOrEmpty(microphoneID))
        {
            Debug.LogError("[MicrophoneRecorder] No microphone available for recording.");
            return;
        }

        isForPitchAnalysis = forPitchAnalysis;
        IsPitchAnalyzeComplete = false; 
        IsRecognitionComplete = false; // 음성 인식 상태 초기화

        Debug.Log($"[MicrophoneRecorder] Start recording for {(isForPitchAnalysis ? "Pitch Analysis" : "Speech Recognition")}...");
        recording = Microphone.Start(microphoneID, false, recordingLengthSec, recordingHZ);

        if (recording == null)
        {
            Debug.LogError("[MicrophoneRecorder] Failed to start recording.");
        }
        else
        {
            Debug.Log("[MicrophoneRecorder] Recording started successfully.");
        }
    }

    /// <summary>
    /// 녹음 중지 및 처리
    /// </summary>
    public void StopRecording()
    {
        if (recording == null || !Microphone.IsRecording(microphoneID))
        {
            Debug.LogError("[MicrophoneRecorder] Recording has not started or is already stopped.");
            return;
        }

        Microphone.End(microphoneID);
        Debug.Log("[MicrophoneRecorder] Recording stopped.");

        if (isForPitchAnalysis)
        {
            AnalyzePitch(recording);
        }
        else
        {
            StartCoroutine(PostVoice(apiUrl, recording));
        }
    }

    /// <summary>
    /// 음성 데이터를 서버로 전송하여 음성 인식 수행
    /// </summary>
    private IEnumerator PostVoice(string url, AudioClip audioClip)
    {
        Debug.Log("[MicrophoneRecorder] Preparing audio data for server...");

        byte[] wavData = GetWavData(audioClip);
        Debug.Log($"[MicrophoneRecorder] WAV data size: {wavData.Length} bytes.");

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(wavData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/octet-stream");
            request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", apiKeyId);
            request.SetRequestHeader("X-NCP-APIGW-API-KEY", apiKeySecret);

            Debug.Log("[MicrophoneRecorder] Sending audio data to server...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[MicrophoneRecorder] Error: {request.error}");
            }
            else
            {
                string response = request.downloadHandler.text;
                HandleServerResponse(response);
            }
        }
    }

    /// <summary>
    /// 서버 응답 처리
    /// </summary>
    private void HandleServerResponse(string jsonResponse)
    {
        try
        {
            var voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(jsonResponse);
            Debug.Log($"[MicrophoneRecorder] Recognized text: {voiceRecognize.text}");
            
            GlobalVariables.PlayerName = voiceRecognize.text;

            if (resultText != null)
            {
                resultText.text = $"만나서 반가워!\n 너의 이름은 {voiceRecognize.text}이구나!";
            }

            IsRecognitionComplete = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MicrophoneRecorder] Failed to parse server response: {e.Message}");
        }
    }

    /// <summary>
    /// 오디오 데이터를 WAV 포맷으로 변환
    /// </summary>
    private byte[] GetWavData(AudioClip audioClip)
    {
        using (var stream = new MemoryStream())
        {
            int headerSize = 44;
            WriteWavHeader(stream, audioClip.samples, recordingHZ, audioClip.channels, 16);

            float[] samples = new float[audioClip.samples];
            audioClip.GetData(samples, 0);

            foreach (var sample in samples)
            {
                short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                stream.Write(BitConverter.GetBytes(intSample), 0, 2);
            }

            return stream.ToArray();
        }
    }

    /// <summary>
    /// WAV 헤더 작성
    /// </summary>
    private void WriteWavHeader(MemoryStream stream, int samples, int sampleRate, int channels, int bitDepth)
    {
        int byteRate = sampleRate * channels * (bitDepth / 8);
        int blockAlign = channels * (bitDepth / 8);

        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(samples * blockAlign + 36), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(BitConverter.GetBytes((short)blockAlign), 0, 2);
        stream.Write(BitConverter.GetBytes((short)bitDepth), 0, 2);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(samples * blockAlign), 0, 4);
    }

    /// <summary>
/// 피치 분석 및 고주파 시간 구간 검출
/// </summary>
public List<float> AnalyzePitch(AudioClip audioClip, float thresholdFrequency = 1000f)
{
    if (audioClip == null)
    {
        Debug.LogError("[MicrophoneRecorder] AudioClip is null. Cannot analyze pitch.");
        return null;
    }

    List<float> pitchSequence = new List<float>();
    int windowSize = 4096; // 샘플 크기를 2의 제곱으로 설정 (FFT 정확도 향상)
    float[] samples = new float[audioClip.samples];
    audioClip.GetData(samples, 0);

    int sampleRate = audioClip.frequency; // 샘플링 레이트
    float maxFreq = sampleRate / 2f; // 최대 주파수
    float minFreq = 100f; // 필터 최소 주파수
    float maxValidFreq = 600f; // 필터 최대 주파수
    float timePerFrame = (float)windowSize / sampleRate; // 프레임당 시간
    float totalHighPitchDuration = 0f; // 하이피치 누적 시간

    // 프레임 단위로 피치 분석
    for (int i = 0; i < samples.Length; i += windowSize)
    {
        float[] frame = new float[windowSize];
        for (int j = 0; j < windowSize && i + j < samples.Length; j++)
        {
            frame[j] = samples[i + j];
        }

        // FFT 수행 및 주요 주파수 추출
        float[] spectrum = PerformFFT(frame);
        float peakFrequency = GetPeakFrequency(spectrum, sampleRate);

        // 필터: 비정상적으로 낮거나 높은 주파수 제외
        if (peakFrequency < minFreq || peakFrequency > maxValidFreq)
        {
            Debug.Log($"[AnalyzePitch] Ignored frequency: {peakFrequency} Hz (out of valid range).");
            continue;
        }

        // 피치 기록
        pitchSequence.Add(peakFrequency);

        // 하이피치 구간 누적 시간 계산
        if (peakFrequency >= thresholdFrequency)
        {
            totalHighPitchDuration += timePerFrame;
        }
    }

    Debug.Log($"[MicrophoneRecorder] Total High Pitch Duration: {totalHighPitchDuration:F2} seconds.");
    
    // GameManager에 값 저장
    totalHighPitchDuration = Mathf.Round(totalHighPitchDuration * 100f) / 100f;
    GameManager.HyperActivity_f.Add(Mathf.RoundToInt(totalHighPitchDuration * 100));
    Debug.Log($"[MicrophoneRecorder] HyperActivity_f set to: {GameManager.HyperActivity_f}");

    IsPitchAnalyzeComplete = true;
    return pitchSequence; // 분석된 피치 데이터 반환
}


/// <summary>
/// FFT 수행
/// </summary>
private float[] PerformFFT(float[] frame)
{
    int n = frame.Length;
    float[] real = new float[n];
    float[] imag = new float[n];

    // 실수 부분 복사 (허수 부분은 0으로 초기화)
    Array.Copy(frame, real, n);

    // FFT 계산
    FFT(real, imag);

    // 진폭 계산
    float[] spectrum = new float[n / 2];
    for (int i = 0; i < n / 2; i++)
    {
        spectrum[i] = Mathf.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
    }

    return spectrum;
}

/// <summary>
/// FFT 알고리즘 수행
/// </summary>
private void FFT(float[] real, float[] imag)
{
    int n = real.Length;
    int m = (int)Math.Log(n, 2);

    // 비트 반전으로 데이터 정렬
    for (int i = 0; i < n; i++)
    {
        int j = BitReverse(i, m);
        if (j > i)
        {
            Swap(ref real[i], ref real[j]);
            Swap(ref imag[i], ref imag[j]);
        }
    }

    // 버터플라이 연산
    for (int s = 1; s <= m; s++)
    {
        int mValue = 1 << s;
        float angle = -2 * Mathf.PI / mValue;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        for (int k = 0; k < n; k += mValue)
        {
            float wr = 1.0f;
            float wi = 0.0f;

            for (int j = 0; j < mValue / 2; j++)
            {
                int index1 = k + j;
                int index2 = k + j + mValue / 2;

                float tr = wr * real[index2] - wi * imag[index2];
                float ti = wr * imag[index2] + wi * real[index2];

                real[index2] = real[index1] - tr;
                imag[index2] = imag[index1] - ti;
                real[index1] += tr;
                imag[index1] += ti;

                float tempWr = wr;
                wr = tempWr * cos - wi * sin;
                wi = tempWr * sin + wi * cos;
            }
        }
    }
}

/// <summary>
/// 비트 반전 계산
/// </summary>
private int BitReverse(int x, int bits)
{
    int reversed = 0;
    for (int i = 0; i < bits; i++)
    {
        reversed = (reversed << 1) | (x & 1);
        x >>= 1;
    }
    return reversed;
}

/// <summary>
/// 주파수 피크 추출
/// </summary>
private float GetPeakFrequency(float[] spectrum, int sampleRate)
{
    int maxIndex = 0;
    float maxAmplitude = 0;

    for (int i = 0; i < spectrum.Length; i++)
    {
        if (spectrum[i] > maxAmplitude)
        {
            maxAmplitude = spectrum[i];
            maxIndex = i;
        }
    }

    // 피크 주파수 계산
    float maxFreq = sampleRate / 2f;
    return (maxIndex / (float)spectrum.Length) * maxFreq;
}

/// <summary>
/// 두 값 교환
/// </summary>
private void Swap(ref float a, ref float b)
{
    float temp = a;
    a = b;
    b = temp;
}


// 스펙트럼에서 주요 주파수 추출
    private float GetDominantFrequency(float[] spectrum)
    {
        int index = 0;
        float maxAmplitude = 0;

        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > maxAmplitude)
            {
                maxAmplitude = spectrum[i];
                index = i;
            }
        }

        // 주요 주파수 계산
        return index * (16000 / (float)spectrum.Length);
    }
    
    public AudioClip GetLastRecording()
    {
        if (recording == null)
        {
            Debug.LogError("[MicrophoneRecorder] No recording available.");
        }
        return recording;
    }


    [Serializable]
    public class VoiceRecognize
    {
        public string text;
    }
}
