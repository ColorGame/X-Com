using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingLinkMonoBehaviour : MonoBehaviour // Ссылка для поиска пути // Прикрипленна в сцене к объекту PathfindingLink
{

    public Vector3 linkPositionA;
    public Vector3 linkPositionB;


    public PathfindingLink GetPathfindingLink() //Получить Ссылку для поиска пути
    {
        return new PathfindingLink
        {// получим сеточные позиции по мировым координатам
            gridPositionA = LevelGrid.Instance.GetGridPosition(linkPositionA),
            gridPositionB = LevelGrid.Instance.GetGridPosition(linkPositionB)
        };
    }

}
