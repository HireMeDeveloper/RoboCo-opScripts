using UnityEngine;

public class LocalDataManager : SingletonBehaviour<LocalDataManager> {
    public int GetLevelValue(LevelName level) {
#if UNITY_EDITOR
        PlayerPrefs.SetInt(level.ToString(), 0);
        return 0;
#endif
        if (PlayerPrefs.HasKey(level.ToString())) {
            return PlayerPrefs.GetInt(level.ToString());
        } else {
            PlayerPrefs.SetInt(level.ToString(), 0);
            return 0;
        }
    }

    public void SetLevelValue(LevelName level, int value) {
        PlayerPrefs.SetInt(level.ToString(), value);
    }

    public bool GetCosmeticValue(CosmeticData data) {

#if UNITY_EDITOR
        PlayerPrefs.SetInt(data.displayName, 0);
        return false;
#endif
        if (PlayerPrefs.HasKey(data.displayName)) {
            return PlayerPrefs.GetInt(data.displayName) > 0;
        } else {
            PlayerPrefs.SetInt(data.displayName, 0);
            return false;
        }
    }

    public void SetCosmeticValue(CosmeticData data, bool value) {
        PlayerPrefs.SetInt(data.displayName, (value) ? 1 : 0);
    }
}
