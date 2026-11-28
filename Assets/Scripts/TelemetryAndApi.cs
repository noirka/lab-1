using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TelemetryAndApi : MonoBehaviour
{
  [Serializable]
  private class WeatherApiResponse
  {
    public CurrentBlock current;
    public DailyBlock daily;
  }

  [Serializable]
  private class CurrentBlock
  {
    public string time;
    public float temperature_2m;
    public int relative_humidity_2m;
    public float wind_speed_10m;
    public int weather_code;
  }

  [Serializable]
  private class DailyBlock
  {
    public string[] time;
    public int[] weather_code;
    public float[] temperature_2m_max;
    public float[] temperature_2m_min;
  }

  [Serializable]
  private class MazeRunReport
  {
    public string sessionId;
    public float durationSeconds;
    public int uniqueNotesViewed;
    public List<string> viewedNoteIds;
    public int ghostItemsCollected;
    public string startedAtUtc;
    public string completedAtUtc;
  }

  [Header("Ендпоінти REST API")]
  [SerializeField]
  private string weatherGetUrl =
    "https://api.open-meteo.com/v1/forecast?latitude=50.4501&longitude=30.5234" +
    "&current=temperature_2m,relative_humidity_2m,weather_code,wind_speed_10m" +
    "&daily=weather_code,temperature_2m_max,temperature_2m_min&timezone=auto&forecast_days=2";
  [SerializeField]
  private string reportPostUrl =
    "https://httpbin.org/post";

  [Header("Панель погоди / прогнозу (GET)")]
  [SerializeField] private GameObject weatherPanel;
  [SerializeField] private TMP_Text weatherDataText;
  [SerializeField] private TMP_Text weatherStatusText;
  [SerializeField] private Transform viewReference;
  [SerializeField] private float distanceFromView = -0.5f;
  [SerializeField] private Vector3 weatherLocalOffset = new Vector3(0f, -0.05f, 0f);

  [Header("Фінальна панель результатів та POST")]
  [SerializeField] private GameObject finishSummaryPanel;
  [SerializeField] private TMP_Text finishSummaryText;
  [SerializeField] private TMP_Text postStatusText;
  [SerializeField] private XRSocketInteractor stickSocket;
  [SerializeField] private Transform finishAnchor;
  [SerializeField] private Vector3 finishOffset = new Vector3(0f, 0.45f, -0.3f);

  [Header("Ідентифікатор сесії")]
  [SerializeField] private string sessionIdPrefix = "maze-run";

  private readonly HashSet<string> viewedNotes = new HashSet<string>();
  private int ghostItemsCollected = 0;

  private float runStartTime = -1f;
  private bool isRunActive = false;
  private bool reportSentForCurrentRun = false;
  private string currentSessionId = string.Empty;
  private string runStartedAtUtc = string.Empty;

  private void Awake()
  {
    if (viewReference == null && Camera.main != null)
      viewReference = Camera.main.transform;

    if (weatherPanel != null)
      weatherPanel.SetActive(false);

    if (finishSummaryPanel != null)
      finishSummaryPanel.SetActive(false);

    currentSessionId = $"{sessionIdPrefix}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
    runStartedAtUtc = DateTime.UtcNow.ToString("o");
  }

  private void OnEnable()
  {
    if (stickSocket != null)
      stickSocket.selectEntered.AddListener(OnStickSnappedIntoSocket);
  }

  private void OnDisable()
  {
    if (stickSocket != null)
      stickSocket.selectEntered.RemoveListener(OnStickSnappedIntoSocket);
  }

  public void StartMazeRunTimer()
  {
    viewedNotes.Clear();
    ghostItemsCollected = 0;
    runStartTime = Time.time;
    isRunActive = true;
    reportSentForCurrentRun = false;
    currentSessionId = $"{sessionIdPrefix}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
    runStartedAtUtc = DateTime.UtcNow.ToString("o");

    if (finishSummaryPanel != null)
      finishSummaryPanel.SetActive(false);
  }

  public void RecordNoteViewed(string noteId)
  {
    if (string.IsNullOrWhiteSpace(noteId))
      return;

    viewedNotes.Add(noteId);
  }

  public void RecordGhostItemCollected()
  {
    ghostItemsCollected++;
  }

  public void ShowWeatherPanelAndFetch()
  {
    if (weatherPanel == null)
      return;

    weatherPanel.SetActive(true);
    PositionPanelRelativeToView(weatherPanel, weatherLocalOffset);
    StartCoroutine(FetchWeatherRoutine());
  }

  public void RefreshWeather()
  {
    if (weatherPanel != null && !weatherPanel.activeSelf)
    {
      weatherPanel.SetActive(true);
      PositionPanelRelativeToView(weatherPanel, weatherLocalOffset);
    }

    StartCoroutine(FetchWeatherRoutine());
  }

  public void HideWeatherPanel()
  {
    if (weatherPanel != null)
      weatherPanel.SetActive(false);
  }

  private void OnStickSnappedIntoSocket(SelectEnterEventArgs args)
  {
    if (reportSentForCurrentRun)
      return;

    float elapsedSeconds = isRunActive && runStartTime >= 0f
      ? Mathf.Max(0f, Time.time - runStartTime)
      : 0f;

    isRunActive = false;
    reportSentForCurrentRun = true;

    string completedUtc = DateTime.UtcNow.ToString("o");
    string startUtc = string.IsNullOrEmpty(runStartedAtUtc) ? completedUtc : runStartedAtUtc;

    MazeRunReport report = new MazeRunReport
    {
      sessionId = currentSessionId,
      durationSeconds = (float)Math.Round(elapsedSeconds, 2),
      uniqueNotesViewed = viewedNotes.Count,
      viewedNoteIds = new List<string>(viewedNotes),
      ghostItemsCollected = ghostItemsCollected,
      startedAtUtc = startUtc,
      completedAtUtc = completedUtc
    };

    ShowFinishSummaryPanel(report);
    StartCoroutine(SendReportPostRoutine(report));
  }

  private void ShowFinishSummaryPanel(MazeRunReport report)
  {
    if (finishSummaryPanel == null)
      return;

    if (finishAnchor != null)
    {
      finishSummaryPanel.transform.position = finishAnchor.position + finishOffset;
      if (viewReference != null)
      {
        Vector3 dir = finishSummaryPanel.transform.position - viewReference.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
          finishSummaryPanel.transform.rotation = Quaternion.LookRotation(dir);
      }
    }

    finishSummaryPanel.SetActive(true);

    if (finishSummaryText != null)
    {
      string startShort = FormatShortUtcTime(report.startedAtUtc);
      string endShort = FormatShortUtcTime(report.completedAtUtc);

      finishSummaryText.text =
        $"Сесія: {report.sessionId}\n" +
        $"Час проходження: {report.durationSeconds:F1} с\n" +
        $"Переглянуто записок: {report.uniqueNotesViewed}\n" +
        $"Зібрано GhostItem: {report.ghostItemsCollected}\n" +
        $"Початок (UTC): {startShort}\n" +
        $"Завершення (UTC): {endShort}";
    }
  }

  private string FormatShortUtcTime(string isoUtc)
  {
    if (DateTime.TryParse(isoUtc, out DateTime parsed))
      return parsed.ToUniversalTime().ToString("HH:mm:ss");

    return isoUtc;
  }

  private IEnumerator FetchWeatherRoutine()
  {
    SetWeatherStatus("Завантаження прогнозу...", Color.yellow);

    using (UnityWebRequest request = UnityWebRequest.Get(weatherGetUrl))
    {
      request.timeout = 10;
      yield return request.SendWebRequest();

      if (request.result != UnityWebRequest.Result.Success)
      {
        SetWeatherStatus($"Помилка з'єднання: {request.error}", Color.red);
        yield break;
      }

      try
      {
        WeatherApiResponse data = JsonUtility.FromJson<WeatherApiResponse>(request.downloadHandler.text);
        if (data != null && data.current != null)
        {
          string formattedTime = data.current.time.Replace("T", " ");
          string currentCondition = DecodeWeatherCode(data.current.weather_code);

          StringBuilder sb = new StringBuilder();
          sb.AppendLine($"Зараз ({formattedTime}): {currentCondition}");
          sb.AppendLine($"Темп.: {data.current.temperature_2m:F1} °C | Вологість: {data.current.relative_humidity_2m}%");
          sb.AppendLine($"Вітер: {data.current.wind_speed_10m:F1} км/год");
          sb.AppendLine($"Оновлено о: {DateTime.Now:HH:mm:ss}");

          if (data.daily != null && data.daily.time != null && data.daily.time.Length > 1)
          {
            string tomorrowCondition = DecodeWeatherCode(data.daily.weather_code[1]);
            float minT = data.daily.temperature_2m_min[1];
            float maxT = data.daily.temperature_2m_max[1];
            sb.AppendLine();
            sb.AppendLine($"Завтра ({data.daily.time[1]}): {tomorrowCondition}");
            sb.Append($"Температура: {minT:F1} ... {maxT:F1} °C");
          }

          if (weatherDataText != null)
            weatherDataText.text = sb.ToString();

          SetWeatherStatus("Прогноз оновлено (GET)", Color.green);
        }
        else
        {
          SetWeatherStatus("Помилка формату відповіді", Color.red);
        }
      }
      catch (Exception ex)
      {
        SetWeatherStatus($"Помилка обробки JSON: {ex.Message}", Color.red);
      }
    }
  }

  private IEnumerator SendReportPostRoutine(MazeRunReport report)
  {
    SetPostStatus("Відправка звіту (POST)...", Color.yellow);

    string jsonBody = JsonUtility.ToJson(report);
    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

    using (UnityWebRequest request = new UnityWebRequest(reportPostUrl, UnityWebRequest.kHttpVerbPOST))
    {
      request.uploadHandler = new UploadHandlerRaw(bodyRaw);
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Content-Type", "application/json");
      request.timeout = 10;

      yield return request.SendWebRequest();

      if (request.result == UnityWebRequest.Result.Success)
      {
        SetPostStatus($"Звіт надіслано (HTTP {request.responseCode})", Color.green);
      }
      else
      {
        SetPostStatus($"Помилка відправки: {request.error}", Color.red);
      }
    }
  }

  private string DecodeWeatherCode(int code)
  {
    if (code == 0) return "Ясно";
    if (code == 1 || code == 2) return "Мінлива хмарність";
    if (code == 3) return "Похмуро";
    if (code == 45 || code == 48) return "Туман";
    if (code >= 51 && code <= 57) return "Мряка";
    if (code >= 61 && code <= 67) return "Дощ";
    if (code >= 71 && code <= 77) return "Сніг";
    if (code >= 80 && code <= 82) return "Злива";
    if (code == 85 || code == 86) return "Снігопад";
    if (code >= 95) return "Гроза";
    return $"Код погоди: {code}";
  }

  private void PositionPanelRelativeToView(GameObject panel, Vector3 offset)
  {
    if (viewReference == null)
      return;

    Vector3 targetPos = viewReference.position
                      + viewReference.forward * distanceFromView
                      + viewReference.TransformDirection(offset);

    panel.transform.position = targetPos;

    Vector3 lookDir = panel.transform.position - viewReference.position;
    if (lookDir.sqrMagnitude > 0.0001f)
      panel.transform.rotation = Quaternion.LookRotation(lookDir);
  }

  private void SetWeatherStatus(string message, Color color)
  {
    if (weatherStatusText != null)
    {
      weatherStatusText.text = message;
      weatherStatusText.color = color;
    }
  }

  private void SetPostStatus(string message, Color color)
  {
    if (postStatusText != null)
    {
      postStatusText.text = message;
      postStatusText.color = color;
    }
  }
}