﻿using System;
using System.Collections;
using System.Collections.Generic;
//using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class elevatorScript: MonoBehaviour
{
    #region [Basic Settings]
    public GameObject outsideButton;
    public GameObject floorButton;
    public GameObject floorShower;
    public RectTransform rect;
    public outsideButtonScript outScript;
    public GameObject Text;
    public GameObject UpI;
    public GameObject DownI;
    private Image Up;
    private Image Down;
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
        outScript = outsideButton.GetComponent<outsideButtonScript>();
        Up = UpI.GetComponent<Image>();
        Down = DownI.GetComponent<Image>();

        state = 0;
        min = max = -1;

        //线程
        //Thread thread = new Thread(new ThreadStart(Running));

        this.InvokeRepeating("Move", 0, 0.02f);
    }

    // Update is called once per frame
    void Update()
    {
        string str = "tasks: ";
        foreach (int i in tasks)
        {
            str = str + i.ToString() + ' ';
        }
        str += "\nup:";
        foreach (int i in tasksup)
        {
            str = str + i.ToString() + ' ';
        }
        str += "\ndown:";
        foreach (int i in tasksdown)
        {
            str = str + i.ToString() + ' ';
        }
        Text.GetComponent<Text>().text = str;
    }
    #endregion

    #region [Elevator Message]
    public int current;
    public int state = 0;
    /// <summary>
    /// 每次运行到达新楼层，自动更新当前楼层数current和UI界面的floorShower
    /// </summary>
    /// <param name="c"></param>
    void SetCurrent(int c)
    {
        current = c;
        SetFloorShower(current);
    }

    /// <summary>
    /// 设置好下一步的状态，并根据此电梯继续运行。
    /// </summary>
    void SetState()
    {
        if (state == 0)
        {
            if (min == -1 && max == -1)
            {
                state = 0;
            }
            else
            {
                if (min < current) state = 2;
                else if (max > current) state = 1;
            }
        }
        else if (state == 1)
        {
            if (max > current) state = 1;
            else state = 0;
        }
        else if (state == 2)
        {
            if (min < current && min != -1) state = 2;
            else state = 0;
        }

        SetStateShower();
    }
    #endregion

    #region [Tasks]
    public ArrayList tasks = new ArrayList();
    public ArrayList tasksup = new ArrayList();
    public ArrayList tasksdown = new ArrayList();
    public int min = -1;
    public int max = -1;
    
    //Add task
    void AddTasks(int floor)
    {
        tasks.Add(floor);
        tasks.Sort();
        SetMinMax();
    }
    void AddTasksup(int floor)
    {
        tasksup.Add(floor);
        tasksup.Sort();
        SetMinMax();
    }
    void AddTasksdown(int floor)
    {
        tasksdown.Add(floor);
        tasksdown.Sort();
        SetMinMax();
    }

    //Remove task
    void RemoveTasks(int floor)
    {
        tasks.Remove(floor);
        SetButtonInteractable(current, "tasks");
        SetMinMax();
    }
    void RemoveTasksup(int floor)
    {
        tasksup.Remove(floor);
        SetButtonInteractable(current, "tasksup");
        SetMinMax();
    }
    void RemoveTasksdown(int floor)
    {
        tasksdown.Remove(floor);
        SetButtonInteractable(current, "tasksdown");
        SetMinMax();
    }

    //update min&max
    void SetMinMax()
    {
        int size1 = tasks.Count;
        int size2 = tasksup.Count;
        int size3 = tasksdown.Count;

        int min1 = size1 == 0 ? -1 : (int)tasks[0];
        int max1 = size1 == 0 ? -1 : (int)tasks[size1 - 1];
        int min2 = size2 == 0 ? -1 : (int)tasksup[0];
        int max2 = size2 == 0 ? -1 : (int)tasksup[size2 - 1];
        int min3 = size3 == 0 ? -1 : (int)tasksdown[0];
        int max3 = size3 == 0 ? -1 : (int)tasksdown[size3 - 1];

        min = Min(min1, min2, min3);
        max = Max(max1, max2, max3);
    }

    private int Min(int a, int b, int c)
    {
        int i = 99;
        if (a != -1)
        {
            if (a < i) i = a;
        }
        if (b != -1)
        {
            if (b < i) i = b;
        }
        if (c != -1)
        {
            if (c < i) i = c;
        }
        if (i == 99) return -1;
        else return i;
    }
    private int Max(int a, int b, int c)
    {
        int i = -1;
        if (a > i) i = a;
        if (b > i) i = b;
        if (c > i) i = c;
        return i;
    }
    #endregion

    #region [Movement Controller]
    public bool isAbleToDoors = false;
    public bool isArrived = false;

    bool OnArrived()
    {
        bool flag = false;

        if (tasksup.Contains(current) && (state == 1 || current == min))
        {
            flag = true;
            RemoveTasksup(current);
        }
        else if (tasksdown.Contains(current) && (state == 2 || current == max))
        {
            flag = true;
            RemoveTasksdown(current);
        }

        if (tasks.Contains(current))
        {
            flag = true;
            RemoveTasks(current);
        }

        //开关门
        if (flag)
        {
            isArrived = true;   //电梯不会移动
            isAbleToDoors = true;
            OpenDoor();
        }

        SetState();

        return flag;
    }

    void Move()
    {
        if (isArrived) return;

        //on arrived
        bool flag = false;
        Vector2 pos = rect.anchoredPosition;
        if (pos.y % 25 == 0)
        {
            SetCurrent((int)(pos.y / 25 + 1));
            flag = OnArrived();
        }

        //on move
        if (flag) return;
        if (state == 1)
        {
            MoveUp();
        }
        else if (state == 2)
        {
            MoveDown();
        }
    }

    void MoveUp()
    {
        Vector2 pos = rect.anchoredPosition;
        rect.anchoredPosition = new Vector2(pos.x, pos.y + 1);
    }

    void MoveDown()
    {
        Vector2 pos = rect.anchoredPosition;
        rect.anchoredPosition = new Vector2(pos.x, pos.y - 1);
    }
    #endregion

    #region [UI]
    void SetFloorShower(int current)
    {
        floorShower.GetComponent<Text>().text = current.ToString();
    }
    
    void SetStateShower()
    {
        Color visible = new Color(1, 1, 1, 1);
        Color invisible = new Color(1, 1, 1, 0.2f);

        if (state == 0)
        {
            Up.color = invisible;
            Down.color = invisible;
        }
        else if (state == 1)
        {
            Up.color = visible;
            Down.color = invisible;
        }
        else if (state == 2)
        {
            Up.color = invisible;
            Down.color = visible;
        }
    }

    void SetButtonInteractable(int floor, string str)
    {
        if (str == "tasks")
        {
            floorButton.transform.Find(floor.ToString()).GetComponent<Button>().interactable = true;
        }
        else if(str == "tasksup")
        {
            outsideButton.transform.Find(floor.ToString()).transform.Find("up").GetComponent<Button>().interactable = true;
        }
        else if (str == "tasksdown")
        {
            outsideButton.transform.Find(floor.ToString()).transform.Find("down").GetComponent<Button>().interactable = true;
        }
    }

    public Animator animator;
    
    public void OpenDoor()
    {
        if (isAbleToDoors == false) return;
        animator.SetBool("isOpen", true);
        
        StopCoroutine("afterOpenDoor");
        StartCoroutine("afterOpenDoor", 1.3f);
    }

    private IEnumerator afterOpenDoor(float t)
    {
        yield return new WaitForSeconds(t);//运行到这，暂停t秒

        animator.SetBool("isOpen", false);
        isAbleToDoors = false;
        this.DelayToDo(0.7f, () =>
        {
            isArrived = false;
        });
    }

    public void CloseDoor()
    {
        if (isAbleToDoors == false) return;
        animator.SetBool("isOpen", false);

        isAbleToDoors = false;
        this.DelayToDo(0.7f, () =>
        {
            isArrived = false;
        });
    }
    #endregion

    #region [Delay Tool]
    /// <summary>
    /// 延时指定秒数，执行某些代码
    /// </summary>
    /// <param name="t">延时秒数</param>
    /// <returns></returns>
    IEnumerator Wait(float t)
    {
        yield return new WaitForSeconds(t);//运行到这，暂停t秒

        //t秒后，继续运行下面代码
        print("Time over.");
    }

    #endregion

}

#region [Delay Tool]
public static class StaticUtils
{
    public static Coroutine DelayToDo(this MonoBehaviour mono, float delayTime, Action action, bool ignoreTimeScale = false)
    {
        Coroutine coroutine = null;
        if (ignoreTimeScale)
        {
            coroutine = mono.StartCoroutine(DelayIgnoreTimeToDo(delayTime, action));
        }
        else
        {
            coroutine = mono.StartCoroutine(DelayToInvokeDo(delayTime, action));

        }
        return coroutine;
    }

    public static IEnumerator DelayToInvokeDo(float delaySeconds, Action action)
    {
        yield return new WaitForSeconds(delaySeconds);
        action();
    }

    public static IEnumerator DelayIgnoreTimeToDo(float delaySeconds, Action action)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + delaySeconds)
        {
            yield return null;
        }
        action();
    }

    public static bool IsNullOrEntry(this string str)
    {
        return String.IsNullOrEmpty(str);
    }
}
#endregion