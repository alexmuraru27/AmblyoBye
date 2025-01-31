using System;

public class DichopticMovieSettingsManager
{
    [Serializable]
    public struct DichopticMovieSettingsStruct
    {
        public float BlobClipValue;
        public float BlobScaleValue;
        public float BlobGreyColorValue;
        public float BlobTimerValue;
        public bool IsFilterEyeRight;
    }

    private const string SETTINGS_FILENAME = "DichopticMovieSettings.cfg";

    private DichopticMovieSettingsStruct dichopticMovieSettings;

    public DichopticMovieSettingsManager(float blobClipValue, float blobScaleValue, float blobGreyColorValue, float blobTimerValue, bool isFilterEyeRight)
    {
        dichopticMovieSettings = new DichopticMovieSettingsStruct
        {
            BlobClipValue = blobClipValue,
            BlobScaleValue = blobScaleValue,
            BlobGreyColorValue = blobGreyColorValue,
            BlobTimerValue = blobTimerValue,
            IsFilterEyeRight = isFilterEyeRight
        };
    }

    public void PersistSettings()
    {
        // Persist straight away the settingsStruct
    }

    public bool RestoreSettings()
    {
        bool isSuccessfullyRestored = false;

        // TODO try to read from SettingsHandler, if something goes wrong -> isSuccessfullyRestored stays false and we persist the default settings
        return isSuccessfullyRestored;
    }


    public void SetBlobClipValue(float value)
    {
        dichopticMovieSettings.BlobClipValue = value;
        PersistSettings();
    }

    public void SetBlobScaleValue(float value)
    {
        dichopticMovieSettings.BlobScaleValue = value;
        PersistSettings();
    }

    public void SetBlobGreyColorValue(float value)
    {
        dichopticMovieSettings.BlobGreyColorValue = value;
        PersistSettings();
    }

    public void SetBlobTimerValue(float value)
    {
        dichopticMovieSettings.BlobTimerValue = value;
        PersistSettings();
    }

    public void SetIsFilterEyeRight(bool value)
    {
        dichopticMovieSettings.IsFilterEyeRight = value;
        PersistSettings();
    }

    public float GetBlobClipValue()
    {
        return dichopticMovieSettings.BlobClipValue;
    }

    public float GetBlobScaleValue()
    {
        return dichopticMovieSettings.BlobScaleValue;
    }

    public float GetBlobGreyColorValue()
    {
        return dichopticMovieSettings.BlobGreyColorValue;
    }

    public float GetBlobTimerValue()
    {
        return dichopticMovieSettings.BlobTimerValue;
    }

    public bool GetIsFilterEyeRight()
    {
        return dichopticMovieSettings.IsFilterEyeRight;
    }
}
