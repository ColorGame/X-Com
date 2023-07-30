using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverObject : MonoBehaviour//  Объект Укрытия
{

    [SerializeField] private CoverType _coverType;

    public CoverType GetCoverType()
    {
        return _coverType;
    }
    
    public void SetCoverType(CoverType coverType) 
    {
        _coverType = coverType;
    }
}

public enum CoverType
{
    None,   //Нету
    Half,   //Половинный 
    Full    //Полный
}
