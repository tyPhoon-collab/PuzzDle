using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PiecesEnum;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]

public class Piece : MonoBehaviour
{
    private Vector2Int moveDir = Vector2Int.zero;
    private float fallDistance = 0;
    float rotateWeight = 270f;
    Rigidbody2D rb;
    [SerializeField] GameObject deleteEfk;

    private Vector3 pos;
    private Vector2Int index;

    float moveSpeed = 5.0f;
    private Puzzle puzzleScript;

    private float pieceSize;

    public Vector2Int MoveDir {
        set { this.moveDir = value; }
    }
    public float FallDistance {
        set { 
            this.fallDistance = value;
            if (value <= 0) {
                rb.velocity = Vector3.zero;
                rb.simulated = false;
                return;
            } 
            rb.simulated = true; 
        }
    }
    public Pieces PieceType {get; set;}
    public Effects EfkType  {get; set;}
    
    // Start is called before the first frame update
    void Awake()
    {
        pos = transform.position;

        puzzleScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Puzzle>();
        EfkType = Effects.NONE;

        rb = GetComponent<Rigidbody2D>();
        FallDistance = 0;
        SetFallDir(puzzleScript.FallDir);

        pieceSize = puzzleScript.pieceSize;
    }

    public void SetFallDir(Vector3 fallDir)
    {
        rb.gravityScale = -fallDir.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveDir != Vector2Int.zero) Move();
        if (fallDistance > 0) Fall();
    }

    void Fall(){
        if (Vector3.Distance(pos, transform.position) >= fallDistance) {
            transform.position = pos + puzzleScript.FallDir * fallDistance;
            pos = transform.position;

            FallDistance = 0;
        }
    }

    void Move(){ //コルーチンでやる方法も考えられるが、Lerpでやるとすると割り算が入るのでやめておく。
        Vector3 _moveDir = new Vector3(moveDir.x, moveDir.y);
        transform.position += _moveDir * moveSpeed * Time.deltaTime;
        if (Vector3.Distance(pos, transform.position) >= pieceSize) {   
            index = puzzleScript.GetIndex(pos);
            transform.position = pos + _moveDir * pieceSize;
            pos = transform.position;

            moveDir = Vector2Int.zero;

            puzzleScript.AddLqueIndexs(index);
        }
    }

    public Effects Delete(){
        StartCoroutine("DelayDelete", 1);
        return EfkType;
    }

    public IEnumerator DelayDelete(float rate){
        StartCoroutine(DeleteCoroutine());
        yield return new WaitForSeconds(puzzleScript.deletingTime * rate);
        Instantiate(deleteEfk, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    IEnumerator DeleteCoroutine(){
        while (true) {
            transform.Rotate(Vector3.up, rotateWeight * Time.deltaTime);
            yield return null;
        }
    }
}
