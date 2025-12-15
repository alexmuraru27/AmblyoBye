using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class ConfirmationDialog : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action<bool> _onResult;

    private void Awake()
    {
        yesButton.onClick.AddListener(() => Resolve(true));
        noButton.onClick.AddListener(() => Resolve(false));
        gameObject.SetActive(false);
    }

    public void Show(string message, Action<bool> onResult)
    {
        messageText.text = message;
        _onResult = onResult;
        gameObject.SetActive(true);
    }

    private void Resolve(bool result)
    {
        gameObject.SetActive(false);
        _onResult?.Invoke(result);
        _onResult = null;
    }
}
