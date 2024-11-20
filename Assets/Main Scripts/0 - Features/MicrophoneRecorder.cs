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
    List<float> highFrequencyTimes = new List<float>(); // 고주파 구간 시간 기록
    int windowSize = 1024; // 한 번에 분석할 샘플 수
    float[] samples = new float[audioClip.samples];
    audioClip.GetData(samples, 0);

    int sampleRate = audioClip.frequency; // 샘플링 레이트
    float timePerSample = 1f / sampleRate; // 샘플당 시간

    // 프레임 단위로 피치 분석
    for (int i = 0; i < samples.Length; i += windowSize)
    {
        float[] frame = new float[windowSize];
        for (int j = 0; j < windowSize && i + j < samples.Length; j++)
        {
            frame[j] = samples[i + j];
        }

        // FFT 수행 및 주요 주파수 추출
        float[] spectrum = FFT(frame);
        float dominantFrequency = GetDominantFrequency(spectrum);

        // 피치 기록
        pitchSequence.Add(dominantFrequency);

        // 고주파 검출
        if (dominantFrequency >= thresholdFrequency)
        {
            float startTime = i * timePerSample;
            float endTime = (i + windowSize) * timePerSample;

            // 고주파 시간 기록
            highFrequencyTimes.Add(startTime);
            highFrequencyTimes.Add(endTime);

            Debug.Log($"High frequency detected: {dominantFrequency} Hz from {startTime:F2}s to {endTime:F2}s");
        }
    }

    Debug.Log("[MicrophoneRecorder] Pitch analysis complete.");
    Debug.Log("[MicrophoneRecorder] High frequency detection complete.");

    // 디버그 출력 (고주파 구간)
    if (highFrequencyTimes.Count > 0)
    {
        Debug.Log("High Frequency Intervals:");
        for (int i = 0; i < highFrequencyTimes.Count; i += 2)
        {
            Debug.Log($"Start: {highFrequencyTimes[i]}s, End: {highFrequencyTimes[i + 1]}s");
        }
    }
    else
    {
        Debug.Log("No high frequency intervals detected.");
    }
    
    IsPitchAnalyzeComplete = true;
    // 피치 분석 결과 반환
    return pitchSequence;
}


// FFT 수행 (간단한 구현)
    private float[] FFT(float[] data)
    {
        int n = data.Length;
        float[] spectrum = new float[n / 2];

        for (int i = 0; i < n / 2; i++)
        {
            spectrum[i] = Mathf.Abs(data[i]); // 단순 진폭 계산 (라이브러리 추천)
        }

        return spectrum;
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
