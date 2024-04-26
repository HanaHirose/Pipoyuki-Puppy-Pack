using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatoControl : MonoBehaviour
{

    [SerializeField] private Animator anim;
    [SerializeField] private Collider enterTrigger;
    [SerializeField] private Collider exitTrigger;
    private enum HatoState
    {
        idle,
        notice
    }
    HatoState hatoState;

    private void Start()
    {
        hatoState = HatoState.idle;
    }


    private void Update()
    {
        NoticeControl();
        UpdateAnimation();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Inu"))
    //    {
    //        hatoState = HatoState.notice;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Inu"))
    //    {
    //        hatoState = HatoState.idle;
    //    }
    //}


    //ハトの周りに子犬やプレイヤーがいるかでハトがnotice状態になるか決める
    private void NoticeControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] inu = GameObject.FindGameObjectsWithTag("Inu");

        if(PlayerIntersects(player, enterTrigger) || InuIntersects(inu, enterTrigger))
        {
            hatoState = HatoState.notice;
        }
        else if(!PlayerIntersects(player, exitTrigger) && !InuIntersects(inu, exitTrigger))
        {
            hatoState = HatoState.idle;
        }

    }

    //プレイヤーのコライダーとハトの子オブジェクトにしたトリガー用のスフィアコライダーが被っているかの判定
    private bool PlayerIntersects(GameObject gameObject, Collider trigger)
    {
        if (trigger.bounds.Intersects(gameObject.GetComponent<Collider>().bounds))
        {
            return true;
        }

        return false;
    }

    //複数いる子犬たちののコライダーとハトの子オブジェクトにしたトリガー用のスフィアコライダーが被っているかの判定
    private bool InuIntersects(GameObject[] gameObjects, Collider trigger)
    {
        foreach (var go in gameObjects)
        {
            if (trigger.bounds.Intersects(go.GetComponent<Collider>().bounds))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateAnimation()
    {
        anim.SetInteger("state", (int)hatoState);
    }


}
