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
        if (!RestoreSettings())
        {
            StoreSettings();
        }
    }

    public void StoreSettings()
    {
        SettingsHandler.StoreSettings(SETTINGS_FILENAME, dichopticMovieSettings);
    }

    public bool RestoreSettings()
    {
        return SettingsHandler.RestoreSettings(SETTINGS_FILENAME, ref dichopticMovieSettings);
    }


    public void SetBlobClipValue(float value)
    {
        dichopticMovieSettings.BlobClipValue = value;
        StoreSettings();
    }

    public void SetBlobScaleValue(float value)
    {
        dichopticMovieSettings.BlobScaleValue = value;
        StoreSettings();
    }

    public void SetBlobGreyColorValue(float value)
    {
        dichopticMovieSettings.BlobGreyColorValue = value;
        StoreSettings();
    }

    public void SetBlobTimerValue(float value)
    {
        dichopticMovieSettings.BlobTimerValue = value;
        StoreSettings();
    }

    public void SetIsFilterEyeRight(bool value)
    {
        dichopticMovieSettings.IsFilterEyeRight = value;
        StoreSettings();
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
