using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour// ����������� ��������� � ���������������� ���������� (������� ���������� � canvas ������� �� �������� � ��������, ������� ����������� ����� �������������� ������ ���� - ������� ���������. ��������� ������ ����� ������ Canvas)
{                                       // � background � text ��������� ���� ������ ������� Raycast target - ��� �� ��������� �� �������

    public static TooltipUI Instance { get; private set; }//(�������������� ������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                          // instance - ���������, � ��� ����� ���� ��������� ResourceManager ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.
                                                          // static ��� ������ ��, ��� ������ �������� ����������� �� ����������� ������� ������, � ����� ������, ��� ���� ������. ������� �������, ���� ������� ���� ������ ����������� �������, � � ������� ����������� ������� ���� ��� �� ���� ����� ������� ����, �� ����������� ���� ���� ��� ����� ������.

    [SerializeField] private RectTransform canvasRectTransform; // ��������� ������ 

    private RectTransform rectTransform; // ��������� ����������� ��������� TooltipUI // � ���������� ��������� ����� �� ������ ����� ����, ��� ���������� (0, 0) ��� �� �� ��������� �������� �� ����� 
    private TextMeshProUGUI textMeshPro; // ����� ��������� (��� TextMeshProUGUI ���� ������� � ������� UI)
    private RectTransform backgroundRectTransform; // ��������� ������� ����
    private TooltipTimer tooltipTimer; // ����� ����������� ��������� (����������� �����)

    private void Awake()
    {
        Instance = this; //�������� ��������� ������

        rectTransform = GetComponent<RectTransform>();
        textMeshPro = transform.Find("text").GetComponent<TextMeshProUGUI>();
        backgroundRectTransform = transform.Find("background").GetComponent<RectTransform>();

        Hide(); // ������ ���������
    }

    private void Update()
    {
        HandleFollowMouse(); // ��������� ���������� �� �����

        if (tooltipTimer != null) // ���� ������ ����� �� - �������� ������
        {
            tooltipTimer.timer -= Time.deltaTime;
            if (tooltipTimer.timer <= 0) //�� ��������� ������� ������ ���������
            {
                Hide();
            }
        }
    }

    private void HandleFollowMouse()// ��������� ���������� �� �����
    {
        // � Canvas - ������ ���� ���� ��������������� Canvas Scale, ������� �� ��������� 1280*720. ��� ��������� �������� Game ����� ���������� ��������� Scale(�������) �� ������
        // ��� �� ��������� ��������� ����� �� ����� ���� ��������� Scale ������
        Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x; //������� ���� ������� �� ������� ������ (����� ������������ ������ � ��������� �.�. Y Z �������� ���������������)

        // ����������� ��� �� ��������� ������ ���������� �� ������
        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width) // ���� ��� ��������� ������� �� ������ ������� ������ �� ...
        {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width; // ����������� �� ������ ������� 
        }
        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }

        rectTransform.anchoredPosition = anchoredPosition; // ������� �������� ����� "����������� ���������" � �������� ����
    }

    private void SetText(string tooltipText) // ��������� ����� � ���������
    {
        textMeshPro.SetText(tooltipText);
        textMeshPro.ForceMeshUpdate(); // �������������� ���������� ����� � ���� �����

        Vector2 textSize = textMeshPro.GetRenderedValues(false); // ������� ������ ������ (false ����� ��������� ��� �������)
        Vector2 padding = new Vector2(8, 8); // ���������� (��� �� ��� ������ �� ������) 
        backgroundRectTransform.sizeDelta = textSize + padding; // ������� ������ ������� ���� � ����������� �� ����� ������
    }

    public void Show(string tooltipText, TooltipTimer tooltipTimer = null) // ����������� ��������� (����� -������� ���� ����������, ����� ����������� ���������)
                                                                           // ����������  "= null" ������ ���� �������� �� ������������, ��������� ���������� ������ �� �����
    {
        this.tooltipTimer = tooltipTimer;
        gameObject.SetActive(true);
        SetText(tooltipText);
        HandleFollowMouse(); // ������� ������������ ���������
    }

    public void Hide() // ������� ���������
    {
        gameObject.SetActive(false);
    }


    public class TooltipTimer // �������� �����
    {
        public float timer;
    }
}
