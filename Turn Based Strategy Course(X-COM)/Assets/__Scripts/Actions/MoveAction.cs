//#define PATHFINDING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// Обратите внимание на эту строку, если она опущена, скрипт не будет знать, что класс 'Path' существует, и он выдаст ошибки компилятора
// Эта строка всегда должна присутствовать в верхней части скриптов, использующих поиск пути
//using Pathfinding;



public class MoveAction : BaseAction // Действие перемещения НАСЛЕДУЕТ класс BaseAction // ВЫделим в отдельный класс // Лежит на каждом юните
{

    public event EventHandler OnStartMoving; // Начал двигаться (когда юнит начнет движение мы запустим событие Event)
    public event EventHandler OnStopMoving; // Прекратил движение (когда юнит законсит движение мы запустим событие Event)
    public event EventHandler<OnChangeFloorsStartedEventArgs> OnChangedFloorsStarted; // Начали менять этажи 
    public class OnChangeFloorsStartedEventArgs : EventArgs // Расширим класс событий, чтобы в аргументе события передать Сеточную позицию Юнита и Целевой позиции
    {
        public GridPosition unitGridPosition; // Откуда прыгаем
        public GridPosition targetGridPosition; // КУда прыгаем
    }


    [SerializeField] private int maxMoveDistance = 5; // Максимальная дистанция движения в сетке

    private List<Vector3> _positionList; // Позиции которые должен преодолеть Юнит (в определенном порядке)
    private int _currentPositionIndex; // Текущая Позиция Индекс
    private bool _isChangingFloors; // это смена этажей
    private float _differentFloorsTeleportTimer; // Таймер телепортации на разные этажи
    private float _differentFloorsTeleportTimerMax = .5f; // Максимальный таймер телепортации на разные этажи (это время воспроизведения анимации прыжка или падения)


#if PATHFINDING
    public Path _path;
    private Seeker _seeker;




    public void Start()
    {
        _seeker = GetComponent<Seeker>();

    }
#endif

    private void Update()
    {
        if (!_isActive) // Если не активны то ...
        {
            return; // выходим и игнорируем код ниже
        }


#if PATHFINDING
        if (_path == null || _positionList.Count == 0) //Если путь не расчитан или лист путой то выходим (StartPath - будет расчитан в следуещем кадре)
        {
            return; // выходим и игнорируем код ниже
        }
#endif
        // Буду двигаться по списку ячеек из _positionList, каждая следующая ячейка будет targetPosition
        Vector3 targetPosition = _positionList[_currentPositionIndex]; // Целевой позицией будет позиция из листа с заданным индексом

        if (_isChangingFloors) // Если надо сменить этаж то
        {
            // Логика остановки и телепортации
            // При подходе к ячейки, с которой юнит будет телепортироваться, необходимо что бы он смотрел в сторону прыжка но только по горизонтали (Не смотрел вверх или вниз)
            Vector3 targetSameFloorPosition = targetPosition; // Целевая позиция этого же Этажа = Целевой позици
            targetSameFloorPosition.y = transform.position.y; // Изменим позицию по оси У как у игрока

            Vector3 rotateDirection = (targetSameFloorPosition - transform.position).normalized; // Направление поворота

            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, rotateDirection, Time.deltaTime * rotateSpeed);

            _differentFloorsTeleportTimer -= Time.deltaTime; //ЗАПУСТИМ Таймер телепортации на разные этажи
            if (_differentFloorsTeleportTimer < 0f) // По истечению таймера // Переключим переключатель этажей и Телепортируемся в целевое положение (а также в момент отсчета таймера будет происходить анимация прыжка)
            {
                _isChangingFloors = false; 
                transform.position = targetPosition; 
            }

        }
        else
        {
            // Обычная логика перемещения

            Vector3 moveDirection = (targetPosition - transform.position).normalized; // Направление движения, еденичный вектор

            float rotateSpeed = 10f; //Чем больше тем быстрее
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed); // поворт юнита. ЗАМЕНИЛ Lerp на - Slerp Сферически интерполирует между кватернионами a и b по соотношению t. Параметр t ограничен диапазоном [0, 1]. Используйте это для создания поворота, который плавно интерполирует между первым кватернионом a и вторым кватернионом b на основе значения параметра t. Если значение параметра близко к 0, выходные данные будут близки к a, если оно близко к 1, выходные данные будут близки к b.

            float moveSpead = 4f; //НУЖНО НАСТРОИТЬ//
            transform.position += moveDirection * moveSpead * Time.deltaTime;
        }
           
        float stoppingDistance = 0.2f; // Дистанция остановки //НУЖНО НАСТРОИТЬ//
        if (Vector3.Distance(transform.position, targetPosition) < stoppingDistance)  // Если растояние до целевой позиции меньше чем Дистанция остановки // Мы достигли цели        
        {
            _currentPositionIndex++; // Увеличим индекс на еденицу

            if (_currentPositionIndex >= _positionList.Count) // Если мы дошли до конца списка тогда...
            {
                SoundManager.Instance.SetLoop(false);
                SoundManager.Instance.Stop();

                OnStopMoving?.Invoke(this, EventArgs.Empty); //Запустим событие Прекратил движение

                ActionComplete(); // Вызовим базовую функцию ДЕЙСТВИЕ ЗАВЕРШЕНО
            }
            else
            {
                targetPosition = _positionList[_currentPositionIndex]; // Целевой позицией будет позиция из листа с заданным индексом
                GridPosition targetGridPosition = LevelGrid.Instance.GetGridPosition(targetPosition); // Получим сеточную позицию Целевой позиции
                GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(transform.position); // Получим сеточную позицию Юнита

                if (targetGridPosition.floor != unitGridPosition.floor) // Если этаж Целевой позииции не совпадает с этажом Юнита то ...
                {
                    // Разные этажи
                    _isChangingFloors = true;
                    _differentFloorsTeleportTimer = _differentFloorsTeleportTimerMax;

                    OnChangedFloorsStarted?.Invoke(this, new OnChangeFloorsStartedEventArgs // Запустим события и передадим сеточные позиции откуда и куда прыгаем
                    {
                        unitGridPosition = unitGridPosition,
                        targetGridPosition = targetGridPosition,
                    });
                }

            }
        }
    }

    // Переопределим TakeAction (Применить Действие (Действовать)) // Мы переименовали Move в TakeAction
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) // Движение к целевой позиции. В аргумент передаем сеточную позицию  и делегат. Вызываю ее для передачи новой целевой позиции
    {
#if PATHFINDING

        _seeker.StartPath(transform.position, LevelGrid.Instance.GetWorldPosition(gridPosition));
        _path = _seeker.GetCurrentPath();
        _positionList = _path.vectorPath;


#else

        List<GridPosition> pathGridPositionList = PathfindingMonkey.Instance.FindPath(_unit.GetGridPosition(), gridPosition, out int pathLength); // Получим список Пути позиций сетки от текущего сеточного положения Юнита до целевого (out int pathLength добавили что бы соответствовала сигнатуре)

        SoundManager.Instance.SetLoop(true);
        SoundManager.Instance.Play(SoundManager.Sound.Move);

       // Надо преобразовать полученный список GridPosition в МИРОВЫЕ КООРДИНАТЫ Vector3
       _positionList = new List<Vector3>(); // Иниацилизируем список Позиции

        foreach (GridPosition pathGridPosition in pathGridPositionList) // переберем компоненты списка pathGridPositionList, преобразуем их в мировые координаты и добавим в _positionList
        {
            _positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition)); // преобразуем pathGridPosition в мировую и добавим в _positionList
        }

#endif

        _currentPositionIndex = 0; // По умолчанию возвращаем к нулю
        OnStartMoving?.Invoke(this, EventArgs.Empty); // Запустим событие Начал двигаться 
        ActionStart(onActionComplete); // Вызовим базовую функцию СТАРТ ДЕЙСТВИЯ // Вызываем этот метод в конце после всех настроек т.к. в этом методе есть EVENT и он должен запускаться после всех настроек
    }

    public override List<GridPosition> GetValidActionGridPositionList() //Получить Список Допустимых Сеточных Позиция для Действий // переопределим базовую функцию
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition(); // Получим позицию в сетке
        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++) // Юнит это центр нашей позиции с координатами unitGridPosition, поэтому переберем допустимые значения в условном радиусе maxMoveDistance
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                for (int floor = -maxMoveDistance; floor <= maxMoveDistance; floor++)
                {

                    GridPosition offsetGridPosition = new GridPosition(x, z, floor); // Смещенная сеточная позиция. Где началом координат(0,0, floor-этаж) является сам юнит 
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition; // Тестируемая Сеточная позиция

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // Проверим Является ли testGridPosition Допустимой Сеточной Позицией если нет то переходим к след циклу
                    {
                        continue; // continue заставляет программу переходить к следующей итерации цикла 'for' игнорируя код ниже
                    }

                    if (unitGridPosition == testGridPosition) // Исключим сеточную позицию где находиться сам юнит
                    {
                        // Таже ячейка на которой стоит юнит :(
                        continue;
                    }

                    if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) // Исключим сеточную позицию где находиться другие юниты
                    {
                        // Позиция занята другим юнитом :(
                        continue;
                    }

                    if (!PathfindingMonkey.Instance.IsWalkableGridPosition(testGridPosition)) //Исключим сеточные позиции где нельзя ходить (есть препятствия стены объекты)
                    {
                        continue;
                    }

#if PATHFINDING
                // НЕ РАБОТАЕТ
                var gg = AstarPath.active.data.gridGraph;

                GridNodeBase Unitnode = gg.GetNode(unitGridPosition.x, unitGridPosition.z);
                GridNodeBase Testnode = gg.GetNode(testGridPosition.x, testGridPosition.z);

                if (PathUtilities.IsPathPossible(Unitnode, Testnode))//Исключим сеточные позиции куда нельзя пройти 
                {
                    continue;
                }

#else

                    if (!PathfindingMonkey.Instance.HasPath(unitGridPosition, testGridPosition)) //Исключим сеточные позиции куда нельзя пройти 
                    {
                        continue;
                    }
#endif

                    int pathfindingDistanceMultiplier = 10; // множитель расстояния определения пути (в классе PathfindingMonkey задаем стоимость смещения по клетке и она равна прямо 10 по диогонали 14, поэтому умножем наш множитель на количество клеток)
                    if (PathfindingMonkey.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier) //Исключим сеточные позиции - Если растояние до тестируемой клетки больше расстояния которое Юнит может преодолеть за один ход
                    {
                        // Длина пути слишком велика
                        continue;
                    }

                    validGridPositionList.Add(testGridPosition); // Добавляем в список те позиции которые прошли все тесты
                                                                 //Debug.Log(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }

    public override string GetActionName() // Присвоить базовое действие //целиком переопределим базовую функцию
    {
        return "движение";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //Получить действие вражеского ИИ // Переопределим абстрактный базовый метод
    {
        int targetCountAtPosition = _unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition); // У юнита вернем скрипт ShootAction и вызовим у него "Получить Количество Целей На Позиции"
                                                                                                           //Я думаю, что самым простым было бы просто иметь метод, который будет подсчитывать врагов в пределах определенного радиуса определенной позиции сетки. Тогда вы можете указать радиус в сериализованном поле и не связывать одно действие с другим (Move и Shoot)
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtPosition * 10 +50, //Ячейка с самым большим количеством стреляемых целей будет в ПРИОРИТЕТЕ. Например если у вас есть позиция сетки, в которой нет стреляемых целей, и другая позиция сетки, в которой есть одна стреляемая цель, ИИ перейдет на вторую позицию сетки, поскольку значение действия основано на количестве стреляемых целей.
        };
        // ВОЗМОЖНЫЕ ВАРИАНТЫ УСЛОЖНЕНИЯ Эта логика может легко учитывать другие факторы… например, если здоровье юнита составляет менее 20%, юнит может пожелать рассмотреть возможность перехода на плитку, на которой НЕТ стреляемых целей.
        // Вы могли бы назначить дополнительный вес плиткам со стреляемыми целями, у которых меньше здоровья, чем плиткам со стреляемыми целями с более высоким здоровьем…
        // Здесь есть много возможностей, помня, конечно, что добавление такой логики может увеличить время, необходимое врагам для расчета наилучших действий.
    }

    //Враги преследовали моих игроков более агрессивно.
    //https://community.gamedev.tv/t/more-aggressive-enemy/220615?_gl=1*ueppqc*_ga*NzQ2MDMzMjI4LjE2NzY3MTQ0MDc.*_ga_2C81L26GR9*MTY3OTE1NDA5Ni4zMS4xLjE2NzkxNTQ1MjYuMC4wLjA.



}
