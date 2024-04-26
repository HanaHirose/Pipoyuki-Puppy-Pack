using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem; // InputAction.CallbackContext contextを使うために必要

public class InuMovement : MonoBehaviour
{

    private Rigidbody rb;
    //private PlayerInputAction playerInputAction;
    public Movement movement; //プレイヤーオブジェクトにアタッチされたスクリプトMovementを参照する（手動でフィールドにドラッグする必要あり）
    [SerializeField] private Animator anim;


    private GameObject player;
    private Camera cam;

    private float followDistance = 1.0f;
    private float avoidDistance = 0.5f;
    private float speed = 2.0f;
    private float avoidRadius = 0.3f;
    public LayerMask inuMask;
    private float rotationSpeed = 5f;
    private float speedPrepareAttack = 20.0f;
    [SerializeField] private float attackForce = 5.0f; //子犬が飛び出すときの力の強さ
    [SerializeField] private float launchAngle = 65.0f; //Launchする時の角度[degree]


    //アニメーション用の状態管理
    private enum MovementState 
    { 
        idle, 
        walk,
        attackPreparing,
        launcing,
        landing
    } 
    MovementState state; //アニメーションの管理に使う

    float moveSpeed;
    //private GameObject selectedPuppy;
    private bool attackStartedBool;

    //子犬の状態を管理。
    public enum PuppyState
    {
        Following,
        AttackPreparing,
        LaunchStart,
        Launching,
        Landing,
        Free
    }
    public PuppyState puppyState = PuppyState.Following; //最初の子犬の状態を指定
    private PuppyState previousPuppyState;

    //private void Awake()
    //{
    //    playerInputAction = new PlayerInputAction();
    //    playerInputAction.Player.Enable();
    //    playerInputAction.Player.Attack.performed += AttackStarted;
    //}


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");

        GameObject cameraObject = GameObject.FindWithTag("MainCamera");
        if(cameraObject != null)
        {
            cam = cameraObject.GetComponent<Camera>();
        }

        movement = player.GetComponent<Movement>(); //プレイヤーのGameObjectからスクリプトMovementを取得
    }


    private void LateUpdate()
    {


        //enumを使って排他的に子犬の状態を管理
        switch(puppyState)
        {
            case PuppyState.Following:
                Follow();
                break;
            case PuppyState.AttackPreparing:
                PuppyAttackPrepare();
                break;
            case PuppyState.LaunchStart:
                PuppyLaunch();
                break;
            case PuppyState.Launching:
                break;
            case PuppyState.Landing:
                break;
            case PuppyState.Free:
                break;
        }



        UpdateAnimation();
    }



    //リーダーについていく動きの制御
    private void Follow()
    {
        //移動速度を計算するために、以前の位置を記録
        Vector3 previousPosition = transform.position;

        Vector3 direction = player.transform.position - transform.position;
        if (direction.magnitude > followDistance)
        {
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime; //プレイヤーへ遠かったら近づく
        }
        else if (direction.magnitude < avoidDistance)
        {
            direction.Normalize();
            transform.position -= direction * speed * Time.deltaTime; //プレイヤーに近づきすぎたら離れる
        }


        Vector3 avoidance = Avoidance();

        if (avoidance != Vector3.zero)
        {
            transform.position += avoidance * speed * Time.deltaTime;　//犬同士が距離を保つ
        }



        Quaternion toRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);


        //移動速度を計算
        moveSpeed = (transform.position - previousPosition).magnitude / Time.deltaTime;

    }




    //犬同士が距離を保つためのベクトルを計算する
    Vector3 Avoidance()
    {
        Collider[] nearInu = Physics.OverlapSphere(transform.position, avoidRadius, inuMask);
        Vector3 avoidance = Vector3.zero;

        foreach(Collider inu in nearInu)
        {
            if(inu.gameObject != gameObject)
            {
                avoidance += (transform.position - inu.transform.position);
            }
        }

        return avoidance;
    }


    //アニメーションを更新する
    private void UpdateAnimation()
    {
        

        if(puppyState == PuppyState.Following)
        {
            if (moveSpeed > 0.1f)
            {
                state = MovementState.walk;
            }
            else
            {
                state = MovementState.idle;
            }

        }
        else if(puppyState == PuppyState.AttackPreparing)
        {
            state = MovementState.attackPreparing;
        }
        else if(puppyState == PuppyState.Launching)
        {
            state = MovementState.launcing;
        }
        else if(puppyState == PuppyState.Landing)
        {
            state = MovementState.landing;
            SetPuppyState(PuppyState.Free); //Landingのアニメーション状態を実行してから子犬の状態をFreeにする
        }
        else if(puppyState == PuppyState.Free)
        {
            state = MovementState.idle;
        }

        anim.SetInteger("state", (int)state);
    }


    

    //enumの子犬の状態を外部スクリプトから変更するための関数
    public void SetPuppyState(PuppyState state)
    {
        //以前の状態を記録
        previousPuppyState = puppyState;
        //状態を新しいものに更新
        puppyState = state;
    }

    //enumの子犬の状態を外部スクリプトから取得するための関数
    public PuppyState GetPuppyState()
    {
        return puppyState;
    }


    //子犬がattackする準備態勢に入る
    private void PuppyAttackPrepare()
    {
        //カメラの向きを参照してプレイヤーの位置からカメラの向きに対して左に犬が配置されるようにする。
        Vector3 cameraLeft = Vector3.Cross(cam.transform.up, cam.transform.forward);
        float offset = 0.25f;
        Vector3 targetPosition = player.transform.position - cameraLeft * offset;

        //子犬がプレイヤーの真横に来るようにする
        //transform.position = targetPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, speedPrepareAttack * Time.deltaTime); //瞬間移動しないで滑らかに移動する
        //子犬がプレイヤーと同じ方向を向くようにする
        //transform.rotation = player.transform.rotation;

        //右スティックの入力向きに応じて子犬の向きを制御する
        Quaternion toRotation = Quaternion.LookRotation(movement.GetPuppyMoveDirection(), Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

    }

    //子犬がattackする
    private void PuppyLaunch()
    {
        if(previousPuppyState != puppyState)
        {
            //ワールド座標系ででz方向のベクトルをxz平面から45度だけ上方向に向ける
            Vector3 upVector = Quaternion.Euler(-launchAngle, 0, 0) * Vector3.forward;
            //上記のベクトルをさらにy軸周りに回転させて子犬が向いてる方向にする
            Vector3 rotatedVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * upVector;

            //子犬に力を加えて飛び出させる
            rb.AddForce(rotatedVector * attackForce, ForceMode.Impulse);

            //子犬の状態をLaunchStartedからLaunchingに切り替える
            SetPuppyState(PuppyState.Launching);
            //プレイヤーと干渉しないレイヤーに子犬を一時的に移す
            gameObject.layer = LayerMask.NameToLayer("NoInteract");

        }
    }






    private void OnCollisionEnter(Collision collision)
    {
        //子犬がLauncing中にGroundに着地するとLanding状態になる処理
        if (puppyState == PuppyState.Launching && collision.gameObject.tag == "Ground")
        {
            SetPuppyState(PuppyState.Landing);
            //子犬のプレイヤーと干渉しないレイヤーから元のレイヤーに戻す
            gameObject.layer = LayerMask.NameToLayer("InuMask");

        }
    }

    

}
