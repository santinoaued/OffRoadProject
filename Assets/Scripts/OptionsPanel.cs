using UnityEngine;
using TMPro;

public class OptionsPanel : MonoBehaviour
{
    [Header("Graphics Quality")]
    [SerializeField] private TMP_Dropdown qualityDropdown;

    private const string QualityPrefKey = "SavedQualityLevel";

    private void Start()
    {
        SetupQualityDropdown();
    }

    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();

        string[] qualityNames = QualitySettings.names;
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(qualityNames));

        int savedLevel = PlayerPrefs.GetInt(QualityPrefKey, QualitySettings.GetQualityLevel());

        savedLevel = Mathf.Clamp(savedLevel, 0, qualityNames.Length - 1);

        qualityDropdown.value = savedLevel;
        qualityDropdown.RefreshShownValue();

        QualitySettings.SetQualityLevel(savedLevel, true);

        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    private void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt(QualityPrefKey, index);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
    }
}
