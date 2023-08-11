using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsUI : MonoBehaviour // Меню настроик 
{

    [SerializeField] private SoundManager soundManager; // Менеджер ЗВУКА
    [SerializeField] private MusicManager musicManager; // Менеджер МУЗЫКИ

    private TextMeshProUGUI soundVolumeText; // Текст громкости звука
    private TextMeshProUGUI musicVolumeText; // Текст громкости музыки

    private void Awake()
    {
        soundVolumeText = transform.Find("soundVolumeText").GetComponent<TextMeshProUGUI>();
        musicVolumeText = transform.Find("musicVolumeText").GetComponent<TextMeshProUGUI>();

        // Найдем кнопки и Добавим событие при нажатии на наши кнопки
        // AddListener() в аргумент должен получить делегат- ссылку на функцию. Функцию будем объявлять АНАНИМНО через лямбду () => {...} 
        transform.Find("soundIncreaseButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            soundManager.IncreaseVolume();
            UpdateText();
        });
        transform.Find("soundDecreaseButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            soundManager.DecreaseVolume();
            UpdateText();
        });

        transform.Find("musicIncreaseButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            musicManager.IncreaseVolume();
            UpdateText();
        });
        transform.Find("musicDecreaseButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            musicManager.DecreaseVolume();
            UpdateText();
        });

        transform.Find("mainMenuButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            Time.timeScale = 1f;// установим скорость игры 1 // Когда вызываем мюню настроек время останавливается(Time.timeScale = 0f), с меню настроек можно перейти в главное меню а от туда обратно в игру но пауза так и не отключена. Поэтому при нажатии на кнопку "главное меню" уберем паузу
            GameSceneManager.Load(GameSceneManager.Scene.MainMenuScene);
        });

        transform.Find("edgeScrollingToggle").GetComponent<Toggle>().onValueChanged.AddListener((bool set) => // Подпишемся на изменение значения Тумблера прокрутка по краям (принимает булевое значение)
        {
            CameraController.Instance.SetEdgeScrolling(set);
        });

        transform.Find("resumeButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            Time.timeScale = 1f; // выключим паузу
            gameObject.SetActive(false); // спрячем меню
        });     
    }

    private void Start()
    {
        UpdateText();
        gameObject.SetActive(false); // спрячем меню

        transform.Find("edgeScrollingToggle").GetComponent<Toggle>().SetIsOnWithoutNotify(CameraController.Instance.GetEdgeScrolling()); // Установим актуальное значения Тумблера прокрутка по краям
    }

    private void UpdateText() // Обновим текс громкости
    {
        // Что бы легче читалось умножим на 10 и округлим до целых
        soundVolumeText.SetText(Mathf.RoundToInt(soundManager.GetVolume() * 10).ToString());
        musicVolumeText.SetText(Mathf.RoundToInt(musicManager.GetVolume() * 10).ToString());
    }

    public void ToggleVisible() // Переключатель видимости меню НАСТРОЙКИ (будем вызывать через инспектор кнопкой OptionsButton)
    {
        gameObject.SetActive(!gameObject.activeSelf); // Переключим в противоположное состояние

        if (gameObject.activeSelf) // Если меню активированно то
        {
            Time.timeScale = 0f; // Поставим игру на паузу
        }
        else // В противном случае оставим нормальную скорость игры
        {
            Time.timeScale = 1f; 
        }
    }

}
