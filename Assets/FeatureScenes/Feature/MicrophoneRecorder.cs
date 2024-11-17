using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // TextMeshPro를 사용하기 위해 추가
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
    private TextMeshProUGUI resultText; // TMP UI를 표시할 TextMeshProUGUI

    private string _microphoneID = null; // 사용할 마이크 ID
    private AudioClip _recording = null; // 녹음된 오디오 클립
    private int _recordingLengthSec = 5; // 녹음 길이 (초)
    private int _recordingHZ = 16000; // 녹음 샘플링 레이트 (STT에 권장되는 값)

    public bool IsRecognitionComplete { get; private set; } = false; // 음성 인식 완료 상태
    public event Action<string> OnRecognitionComplete; // 음성 인식 완료 이벤트

    private void Start()
    {
        // 마이크 확인
        if (Microphone.devices.Length > 0)
        {
            _microphoneID = Microphone.devices[0];
            Debug.Log($"[MicrophoneRecorder] Detected Microphone: {_microphoneID}");
            IsRecognitionComplete = false;
        }
        else
        {
            Debug.LogError("[MicrophoneRecorder] No microphone detected!");
        }
    }

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0 || string.IsNullOrEmpty(_microphoneID))
        {
            Debug.LogError("[MicrophoneRecorder] No microphone available for recording.");
            return;
        }

        Debug.Log("[MicrophoneRecorder] Start recording...");
        IsRecognitionComplete = false; // 플래그 초기화
        _recording = Microphone.Start(_microphoneID, false, _recordingLengthSec, _recordingHZ);

        if (_recording != null)
        {
            Debug.Log("[MicrophoneRecorder] Recording started successfully.");
        }
        else
        {
            Debug.LogError("[MicrophoneRecorder] Failed to start recording.");
        }
    }

    public void StopRecording()
    {
        Debug.Log("[MicrophoneRecorder] StopRecording called.");

        if (_recording == null)
        {
            Debug.LogError("[MicrophoneRecorder] Recording has not started yet!");
            return;
        }

        if (Microphone.IsRecording(_microphoneID))
        {
            Microphone.End(_microphoneID);
            Debug.Log("[MicrophoneRecorder] Microphone recording stopped. Sending audio for recognition...");
        
            // 서버로 전송 시작
            StartCoroutine(PostVoice(apiUrl, _recording));
        }
        else
        {
            Debug.LogError("[MicrophoneRecorder] Microphone is not recording.");
        }
    }


    private IEnumerator PostVoice(string url, AudioClip audioClip)
    {
        Debug.Log("[MicrophoneRecorder] Preparing audio data for server...");

        // AudioClip 데이터를 WAV로 변환
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
                Debug.LogError($"[MicrophoneRecorder] Error sending audio: {request.error}");
            }
            else
            {
                // JSON 응답 처리
                string message = request.downloadHandler.text;
                Debug.Log($"[MicrophoneRecorder] Server response: {message}");

                try
                {
                    VoiceRecognize voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(message);
                    Debug.Log($"[MicrophoneRecorder] Recognized text: {voiceRecognize.text}");

                    // TMP 텍스트에 표시
                    if (resultText != null)
                    {
                        GlobalVariables.PlayerName  = $"{voiceRecognize.text} ";
                        resultText.text = $"만나서 반가워!\n 너의 이름은 {voiceRecognize.text} 이구나!";
                    }

                    // 음성 인식 완료 플래그 설정 및 이벤트 호출
                    IsRecognitionComplete = true;
                    OnRecognitionComplete?.Invoke(voiceRecognize.text);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MicrophoneRecorder] Failed to parse server response: {e.Message}");
                }
            }
        }
    }

    private byte[] GetWavData(AudioClip audioClip)
    {
        Debug.Log("[MicrophoneRecorder] Converting AudioClip to WAV format...");

        MemoryStream stream = new MemoryStream();
        int headerSize = 44; // WAV 헤더 크기
        int sampleRate = audioClip.frequency;
        int channels = audioClip.channels;
        int bitDepth = 16;

        // WAV 헤더 작성
        WriteWavHeader(stream, audioClip.samples, sampleRate, channels, bitDepth);

        // AudioClip 데이터를 가져와서 WAV 포맷으로 변환
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        foreach (float sample in samples)
        {
            short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            stream.Write(BitConverter.GetBytes(intSample), 0, 2);
        }

        Debug.Log("[MicrophoneRecorder] WAV conversion complete.");
        return stream.ToArray();
    }

    private void WriteWavHeader(MemoryStream stream, int samples, int sampleRate, int channels, int bitDepth)
    {
        Debug.Log("[MicrophoneRecorder] Writing WAV header...");
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
        Debug.Log("[MicrophoneRecorder] WAV header written.");
    }

    [Serializable]
    public class VoiceRecognize
    {
        public string text;
    }
}
