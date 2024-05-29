using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PiecesEnum;
using StatusEnum;
using ItemsEnum;

using GameSystem;

public class Puzzle : MonoBehaviour
{
    public enum Direction{
        UP,
        LEFT,
        DOWN,
        RIGHT
    }

    public enum PuzzleType{
        PAZUDORA,
        PUYOPUYO,
        MATCH3,
        CUSTOMIZED
    }

    [SerializeField] int WIDTH = 7;
    [SerializeField] int HEIGHT = 5;

    public PuzzleType selectPuzzleType = PuzzleType.CUSTOMIZED;
    
    public int chainThreshold = 3;
    public int matchChainThreshold = 3;
    public float pieceSize = 2.0f;

    [Range(0f, 5f)] public float deletingTime = 1.0f;
    [Range(0f, 1f)] public float resetDeletingTimeRate = 0.5f;
    [Range(0f, 10f)] public float autoPuzzleInterval = 0f;
    [Range(0f, 5f)] public float deletingInterval = 1.5f;
    [Range(0.1f, 100f)] public float moveDistanceThreshold = 20.0f;

    public bool isGenerateSpecialPiece = false;
    private bool isAutoPuzzle = false;

    public GameObject backgroundPanel; 
    public Transform dependPosTransform;

    private Vector3 fieldPos = Vector3.zero;
    private Vector3 mousePos;
    private Vector3 fallDir;
    private Vector2Int moveDir;
    private Vector2Int selectIndex; //本当はプロパティでやりたい
    private Vector2Int errorIndex = new Vector2Int(-1,-1);

    private int moveableTimes;
    private int additionalMoveableTimes;
    private int combo = 0;
    private int deletePieceSum = 0;

    public ty_Hero tyHero; 
    public ty_Log tyLog;
    public Direction selectFallDir = Direction.DOWN;

    public AudioClip movedAudio;
    public AudioClip deleteAudio;
    public AudioSource audioSource;

    private Piece[,] fieldObjs;
    private Pieces[,] field;
    private Pieces[,] _field;
    bool[,] checkedCell;

    [SerializeField] private GameObject selectPanel;

    [SerializeField] private GameObject[] pieceObjs = new GameObject[(int)Pieces.MAX];
    [SerializeField] private GameObject[] spieceObjs = new GameObject[(int)Pieces.MAX];
    [SerializeField] private GameObject[] spiece2Objs = new GameObject[(int)Pieces.MAX];
    [SerializeField] private GameObject[] rareObjs = new GameObject[(int)Effects.RARE_PIECES_MAX];

    private GameStatus gameStatus = GameStatus.ERROR;

    private List<Vector2Int> lqueIndexs = new List<Vector2Int>();
    private List<Effects> lqueRarePieces = new List<Effects>(); 
    private List<HashSet<Vector2Int>> deleteIndexs = new List<HashSet<Vector2Int>>();
    private List<Pieces> deletePieces = new List<Pieces>();

    public Vector3 FallDir { 
        get { return this.fallDir; }
    }

    public int MoveableTimes {
        get { return this.moveableTimes; }
        set {
            if (value < 0)
            {
                AdditionalMoveableTimes--;
                return;
            }
            this.moveableTimes = value;
            tyLog.UpdateMoveable(value);
        }
    }
    public int AdditionalMoveableTimes
    {
        get { return this.additionalMoveableTimes; }
        set
        {
            this.additionalMoveableTimes = value;
            tyLog.UpdateAdditionalMoveable(value);
        }
    }
    public int DeletePieceSum
    {
        get { return deletePieceSum; }
        set
        {
            this.deletePieceSum = value;
            tyLog.UpdatePieceSum(value);
        }
    }

    public ty_Hero HeroScript {
        get { return this.tyHero; }
    }

    private GameStatus SetGameStatus {
        set {
            this.gameStatus = value;
            tyLog.UpdateGameStatus(value);
            switch (value) {
                case GameStatus.CLICKABLE:
                    StopAllCoroutines();
                    this.combo = 0;
                    break;
                case GameStatus.DELETING:
                    StartCoroutine(DeleteCoroutine());
                    break;
                    
                case GameStatus.RESET:
                    StartCoroutine(ResetCoroutine());
                    break;
            }
        }
    }

    public void AddLqueIndexs(Vector2Int index){
        lqueIndexs.Add(index);
        //Swap。リストの要素数を監視
        if (lqueIndexs.Count == 2){
            SwapPieces();     //マウスドラッグによるスワップを行ってから
            SetGameStatus = GameStatus.DELETING;
        }
    }

    public void AddLqueRarePieces(Effects efk)
    {
        lqueRarePieces.Add(efk);
    }

    public bool IsIndexOutOfRange(int i, int j){
        return (i < 0 || HEIGHT <= i) || (j < 0 || WIDTH <= j);
    }

    public Vector2Int GetIndex(Vector3 offset){
        Vector2Int index = new Vector2Int(
            (int)Mathf.Round((offset.x - fieldPos.x) / pieceSize),  //j
            (int)Mathf.Round((offset.y - fieldPos.y) / pieceSize) //i
        );

        if (!IsIndexOutOfRange(index.y, index.x))
            return index;
        else 
            return errorIndex;
    }

    public void SwitchIsAutoPuzzle(){
        isAutoPuzzle = !isAutoPuzzle;
        if (!isAutoPuzzle && gameStatus == GameStatus.WAIT_INTERVAL) {
            SetGameStatus = GameStatus.CLICKABLE;
        }
    }

    void CheckAddDeleteIndex(HashSet<Vector2Int> indexs, Vector2Int index){
        int i = index.y;
        int j = index.x;
        if (!IsIndexOutOfRange(i, j) && field[i, j] != Pieces.NONE) {
            indexs.Add(index);
        }
    }

    // Start is called before the first frame update
    void Awake(){
        fieldPos = new Vector3(-(int)(WIDTH/2)*pieceSize, -(int)(HEIGHT/2)*pieceSize, 0);
        if(dependPosTransform != null) fieldPos += dependPosTransform.position;

        fieldObjs = new Piece[HEIGHT, WIDTH];
        field = new Pieces[HEIGHT, WIDTH];

        if (matchChainThreshold > chainThreshold) chainThreshold = matchChainThreshold;

        switch (selectPuzzleType){
            case PuzzleType.PAZUDORA : 
                WIDTH = 6;
                HEIGHT = 5;
                chainThreshold = 3;
                matchChainThreshold = 3;
                selectFallDir = Direction.DOWN;
                isGenerateSpecialPiece = false;
                break;
            
            case PuzzleType.PUYOPUYO : 
                WIDTH = 6;
                HEIGHT = 12;
                chainThreshold = 4;
                matchChainThreshold = 1;
                selectFallDir = Direction.DOWN;
                isGenerateSpecialPiece = false;
                break;

            case PuzzleType.MATCH3   : 
                WIDTH = 7;
                HEIGHT = 5;
                chainThreshold = 3;
                matchChainThreshold = 3;
                selectFallDir = Direction.UP;
                isGenerateSpecialPiece = true;
                break;

            default : break;
        }

        Vector2Int _fallDir = GetVector2Int(selectFallDir);
        fallDir = new Vector3(_fallDir.x, _fallDir.y, 0);

        //Error回避用のコードを以下にかく。
        //プレハブにスクリプトがついていなかったら付ける。
        foreach (var pieceObj in pieceObjs) {
            Piece script = pieceObj.GetComponent<Piece>();
            if (script == null) pieceObj.AddComponent<Piece>();
        }

        if (isGenerateSpecialPiece) {
            foreach (var spieceObj in spieceObjs) {
                Piece script = spieceObj.GetComponent<Piece>();
                if (script == null) spieceObj.AddComponent<Piece>();
            }
            foreach (var spiece2Obj in spiece2Objs) {
                Piece script = spiece2Obj.GetComponent<Piece>();
                if (script == null) spiece2Obj.AddComponent<Piece>();
            }
            foreach (var rareObj in rareObjs) {
                Piece script = rareObj.GetComponent<Piece>();
                if (script == null) rareObj.AddComponent<Piece>();
            }
        }

    }

    private void Start(){
        string json = PlayerPrefs.GetString("PuzzleData", JsonUtility.ToJson(new PuzzleSaveData(Functions.initMoveableTimes, 0, 0)));
        PuzzleSaveData saveData = JsonUtility.FromJson<PuzzleSaveData>(json);
        MoveableTimes = saveData.moveable;
        AdditionalMoveableTimes = saveData.additionalMoveable;
        DeletePieceSum = saveData.deletePieceSum;

        InstantiateBackground();
        Init();
    }

    public void Save()
    {
        var data = new PuzzleSaveData(
                MoveableTimes, AdditionalMoveableTimes, DeletePieceSum    
            );
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PuzzleData", json);
    }

    void InstantiateBackground(){
        for (int i = 0; i < HEIGHT; i++)
            for (int j = 0; j < WIDTH; j++) {
                Vector3 pos = GetIndexToPos(i, j);
                Instantiate(backgroundPanel, pos, Quaternion.identity, transform);
            }
    }

    Vector3 GetIndexToPos(int i, int j)
    {
        return new Vector3(j * pieceSize, i * pieceSize, 0) + fieldPos;
    }

    public void Init(){
        deleteIndexs.Clear();
        deletePieces.Clear();
        for (int i = 0; i < HEIGHT; i++)
            for (int j = 0; j < WIDTH; j++) 
                InstantiatePiece(i, j, pieceObjs, GetRandomPieceType());
        
        SetGameStatus = GameStatus.CLICKABLE;
    }
    
    GameObject InstantiatePiece(int i, int j, GameObject[] objs, Pieces piece, Effects effect = Effects.NONE, float dist = 0f){
        Vector3 pos = new Vector3(j * pieceSize, i * pieceSize + dist * -fallDir.y, 0) + fieldPos;
        GameObject obj = objs[GetObjIndex(piece)];
        
        GameObject pieceObj = Instantiate(obj, pos, Quaternion.identity, transform);
        field[i,j] = piece;
        Piece pieceScript = pieceObj.GetComponent<Piece>();
        fieldObjs[i,j] = pieceScript;
        pieceScript.PieceType = piece;
        pieceScript.EfkType = effect;

        return pieceObj;
    }
    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.R) && gameStatus == GameStatus.CLICKABLE)
        {
            SetGameStatus = GameStatus.RESET;
        }

        if (isAutoPuzzle && CanMove()) {
            StartCoroutine("AutoPuzzle");
            return;
        }

        if(Input.GetMouseButtonDown(0)){
            if (CanMove()) {
                mousePos = Input.mousePosition;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

                selectIndex = GetIndex(worldPos);
                if (selectIndex != errorIndex) {
                    selectPanel.transform.position = GetIndexToPos(selectIndex.y, selectIndex.x);
                    selectPanel.SetActive(true);
                    SetGameStatus = GameStatus.WAIT_MOUSE_UP;
                }
            }
        }

        if (Input.GetMouseButtonUp(0)){
            selectPanel.SetActive(false);

            if (gameStatus == GameStatus.WAIT_MOUSE_UP) {
                Vector3 _mousePos = Input.mousePosition;
                Vector2 moveVec = new Vector2(
                    _mousePos.x - mousePos.x,
                    _mousePos.y - mousePos.y
                );

                //ドラッグの距離が大きいならばスワイプ。小さいならばクリックとして判定。
                if (moveVec.sqrMagnitude > Mathf.Pow(moveDistanceThreshold, 2)) {      
                    SetMoveDir(moveVec);
                    Swipe();
                } else {
                    Click();
                }
            }   
        }
    }
    bool CanMove()
    {
        bool haveMoveableTimes = MoveableTimes > 0 || AdditionalMoveableTimes > 0;
        if (!haveMoveableTimes) tyLog.DrawMoveableAttentionImage();
        return gameStatus == GameStatus.CLICKABLE && haveMoveableTimes; 
    }


    void Swipe(){
        Vector2Int index = moveDir + selectIndex;
        if (IsIndexOutOfRange(index.y, index.x)) return;
        MovePieces();
    }

    void Click(){
        if (fieldObjs[selectIndex.y, selectIndex.x].EfkType != Effects.NONE) {
            HashSet<Vector2Int> indexs = new HashSet<Vector2Int>();
            indexs.Add(selectIndex);
            deleteIndexs.Add(indexs);
            deletePieces.Add(Pieces.EFK);
            SetGameStatus = GameStatus.DELETING;
            MoveableTimes--;
            return;
        } 
        SetGameStatus = GameStatus.CLICKABLE;
    }

    void SetMoveDir(Vector2 moveVec){
        if (Mathf.Abs(moveVec.x) >= Mathf.Abs(moveVec.y)){
            if (moveVec.x >= 0) 
                moveDir = Vector2Int.right;
            else 
                moveDir = Vector2Int.left;
        } else {
            if (moveVec.y >= 0)     
                moveDir = Vector2Int.up;
            else 
                moveDir = Vector2Int.down;
        }
    }

    void MovePieces(){
        MoveableTimes--;

        Vector2Int sIndex = selectIndex;
        fieldObjs[sIndex.y, sIndex.x].MoveDir = moveDir;
        sIndex += moveDir;
        fieldObjs[sIndex.y, sIndex.x].MoveDir = -moveDir;

        SetGameStatus = GameStatus.SWAPING;
    }

    void SwapPieces(){
        Vector2Int v1 = lqueIndexs[0];
        Vector2Int v2 = lqueIndexs[1];

        lqueIndexs = new List<Vector2Int>();

        Swap(ref field[v1.y, v1.x], ref field[v2.y, v2.x]);
        Swap(ref fieldObjs[v1.y, v1.x], ref fieldObjs[v2.y, v2.x]);
    }

    void Swap<T> (ref T v1, ref T v2){
        T tmp = v1; 
        v1 = v2;
        v2 = tmp;
    }


    int GetObjIndex(Pieces piece){
        if (piece == Pieces.EFK) {
            int index = (int)lqueRarePieces[0];
            lqueRarePieces.RemoveAt(0);
            return index;
        } else {
            return (int)piece;
        }
    }


    class AddStatus {
        public int Atk {get; set;}
        public int Def {get; set;}
        public int Hp {get; set;}
        public int Money {get; set;}
        public int Hunger {get; set;}
        public int GetSum() { return Atk + Def + Hp + Money + Hunger; }
    }

    void Delete(){
        AddStatus addStatus = new AddStatus();
        CheckDelete();

        int iSum = 0;
        int jSum = 0;
        int sum = 0;
        if (deleteIndexs.Count == 0) return;

        combo++;
        PlaySound(movedAudio);
        
        //リアルタイムでdeleteIndexsが更新される悪いコード
        for (int k = 0; k < deleteIndexs.Count; k++) {
            var indexs = deleteIndexs[k];
            foreach (var index in indexs) {
                int i = index.y; 
                int j = index.x;
                iSum += i;
                jSum += j;
                if (field[i, j] == Pieces.NONE) continue;
                Effects efk = fieldObjs[i, j].Delete();
                DeleteChange(new Vector2Int(j, i), efk);
                CheckAddStatus(field[i, j], addStatus);
                field[i, j] = Pieces.NONE;
            }

            sum += indexs.Count;

        }
        tyLog.DrawCombo(GetIndexToPos(iSum/sum, jSum/sum), combo);
        //まとめてステータスを変更する。Logがきれいになるし、複数回プロパティが呼ばれるより良いはず
        UpdateStatus(addStatus);
    }

    public void DeleteChange(Vector2Int index, Effects efk){
        HashSet<Vector2Int> indexs = new HashSet<Vector2Int>();

        switch (efk) {
            case Effects.BOMB:
                foreach (Direction dir in System.Enum.GetValues(typeof(Direction))) {
                    CheckAddDeleteIndex(indexs, index + GetVector2Int(dir));                    
                }
                deleteIndexs.Add(indexs);
                deletePieces.Add(Pieces.EFK); //暫定的な処理。気を付ける。
                break;
            case Effects.DELETE_SAME_PIECE:
                Pieces piece = field[index.y, index.x];
                checkedCell = new bool[HEIGHT, WIDTH];
                for (int i = 0; i < HEIGHT; i++) {
                    for (int j = 0; j < WIDTH; j++) {
                        if (field[i, j] == piece && !(i == index.y && j == index.x)) {
                            var list = GetChainIndexs(i, j, field[i, j], field);
                            indexs = new HashSet<Vector2Int>(list);
                            deleteIndexs.Add(indexs);
                            deletePieces.Add(Pieces.EFK);
                        }
                    }
                }
                break;
            case Effects.CRYSTAL:
                tyHero.AddCrystal();
                break;
            case Effects.TREASURE:
                tyHero.AddTreasureBox();
                break;
        }
    }

    void CheckAddStatus(Pieces piece, AddStatus addStatus){
        switch (piece) {
            case Pieces.ATK:    addStatus.Atk += 1; break;
            case Pieces.DEF:    addStatus.Def += 1; break;
            case Pieces.HEALTH: addStatus.Hp += 1; break;
            case Pieces.MONEY:  addStatus.Money += 1; break;
            case Pieces.FOOD:   addStatus.Hunger += 1; break; 
        }
    }

    void UpdateStatus(AddStatus addStatus){
        if (addStatus.Atk != 0) 
            tyHero.EquipLvUp(Items.SWORD, addStatus.Atk * combo);
        if (addStatus.Def!= 0) 
            tyHero.EquipLvUp(Items.SHIELD, addStatus.Def * combo);
        tyHero.Hp += addStatus.Hp * combo;
        tyHero.Money += addStatus.Money * combo;
        tyHero.Hunger += addStatus.Hunger * combo;
        DeletePieceSum += addStatus.GetSum();
    }

    void PlaySound(AudioClip audioClip){
        if (audioSource.enabled) {  
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    /*
    以下の3つの関数がパズルの根幹。流れは
    1．再帰で配列の要素同士のつながりをListで保持
    2．Listの要素数を評価
    3．設定を参照し、消えるピースならdeleteIndexsに追加する
    4．どの種類のPieceが消えたかを参照するために、deletePiecesに追加する
    本当は辞書などが直感的かもしれないが、辞書はKeyが複数登録できないので2つの配列で参照することにした。
    関数化してもいいかも。
    */

    void CheckDelete(){
        checkedCell = new bool[HEIGHT, WIDTH];
        for (int i = 0; i < HEIGHT; i++) {
            for (int j = 0; j < WIDTH; j++) {
                var piece = field[i, j];
                var list = GetChainIndexs(i, j, piece, field);
                var indexs = GetDeleteIndexs(list, piece);
                if (indexs.Count >= chainThreshold) {
                    deleteIndexs.Add(indexs);
                    deletePieces.Add(piece);
                }
            } 
        }
    }

    HashSet<Vector2Int> GetDeleteIndexs(List<Vector2Int> list, Pieces piece){
        if (list.Count < chainThreshold) return new HashSet<Vector2Int>(); //listがそもそも少なかったら評価する必要なし

        HashSet<Vector2Int> indexs = new HashSet<Vector2Int>();

        foreach (var index in list) {
            int vSum = 1, hSum = 1;

            //マッチ3に準ずるかチェック
            SetSum(list, index, Vector2Int.up, ref vSum);
            SetSum(list, index, Vector2Int.down, ref vSum);
            SetSum(list, index, Vector2Int.right, ref hSum);
            SetSum(list, index, Vector2Int.left, ref hSum);
            
            if (vSum >= matchChainThreshold || hSum >= matchChainThreshold)
            {
                indexs.Add(index);
            } 
        }
        return indexs;
    }

    void SetSum(List<Vector2Int> list, Vector2Int index, Vector2Int dir, ref int sum)
    {
        for (int n = 1; n < matchChainThreshold; n++)
        {
            if (list.IndexOf(index + dir * n) == -1) break;
            sum++;
        }
    }

    //AIが仮想の盤面を作ってパズルをするときに参照するFieldが違うので、Fieldも引数に取る必要が出てしまった。
    //もし仮に、AIがもとのFieldを変えても差支えないようなコードにすれが、第4引数はいらない。
    //もとのFieldを変えてもいいような仕様にするには、playerが操作不能になり、かつ、変更後もとに戻せばよい。
    //どちらが良いかは分からない。
    List<Vector2Int> GetChainIndexs(int i, int j, Pieces piece, Pieces[,] field){
        if (IsIndexOutOfRange(i, j) || piece != field[i, j] || checkedCell[i, j] || field[i, j] == Pieces.NONE) {
            return new List<Vector2Int>();

        } else {
            checkedCell[i, j] = true;
            if (piece == Pieces.EFK) return new List<Vector2Int>();
            List<Vector2Int> indexs = new List<Vector2Int>();
            indexs.Add(new Vector2Int(j, i));

            indexs.AddRange(GetChainIndexs(i, j+1, piece, field));
            indexs.AddRange(GetChainIndexs(i, j-1, piece, field));
            indexs.AddRange(GetChainIndexs(i+1, j, piece, field));
            indexs.AddRange(GetChainIndexs(i-1, j, piece, field));

            return indexs;
        }
    }

    void GenerateSpecialPiece(){
        for (int i = 0; i < deleteIndexs.Count; i++) {
            if (deletePieces[i] != Pieces.EFK) {
                var indexs = deleteIndexs[i];            
                if (indexs.Count == chainThreshold) continue;
                //たくさん消したときの処理
                Vector2Int tindex = Vector2Int.zero;
                foreach (var index in indexs) {
                    tindex = index;
                    if (index == selectIndex || index == selectIndex + moveDir) break;
                }

                switch (indexs.Count) {
                    case 4:
                        InstantiatePiece(tindex.y, tindex.x, spieceObjs, deletePieces[i], Effects.BOMB); 
                        break;

                    default:
                        InstantiatePiece(tindex.y, tindex.x, spiece2Objs, deletePieces[i], Effects.DELETE_SAME_PIECE);
                        break;         
                }
            }
        }  
    }

    void Fall(){
        if (deleteIndexs.Count > 0) {
            //落ちる側から走査しなければならない。下が空いていたら即座に代入するメソッド
            for (int j = 0; j < WIDTH; j++) 
                for (int i = 1; i < HEIGHT; i++) 
                    CheckFall(i, j);
            
            //盤面を整理したあとで追加のボールを降らせる
            FallNewPieces();

        } else {
            SetGameStatus = GameStatus.CLICKABLE;
        }

        deleteIndexs.Clear();
        deletePieces.Clear();
    }

    void CheckFall(int i, int j){
        if (selectFallDir == Direction.UP) i = HEIGHT - 1 - i;
        if (field[i, j] != Pieces.NONE) { //Ballがあるとき、自分より下の空白(field[i,j]==Pieces.NONE)の数を調べる。これが落ちる距離になる。
            int marginCount = 0;
            int _i = i; 
            int _j = j;
            while (true) {
                _i += (int)fallDir.y;
                _j += (int)fallDir.x;
                if (IsIndexOutOfRange(_i, _j)) break; 
                if (field[_i, _j] == Pieces.NONE) marginCount++;
            }

            if (marginCount > 0) {
                fieldObjs[i, j].GetComponent<Piece>().FallDistance = marginCount * pieceSize;
                Vector2 targetIndex = fallDir * (float)marginCount;
                _i = i + (int)targetIndex.y;
                _j = j + (int)targetIndex.x;
                field[_i, _j] = field[i, j];
                field[i, j] = Pieces.NONE;
                fieldObjs[_i, _j] = fieldObjs[i, j];
            }
        }
    }
    
    void FallNewPieces(){
        for (int j = 0; j < WIDTH; j++) {
            for (int i = 0; i < HEIGHT; i++) {
                if (field[i, j] != Pieces.NONE) continue;

                float dist = HEIGHT * pieceSize; //パズルの画面外から
                //ここにrareQueの中身を生成するようにする。
                GameObject pieceObj;
                if (lqueRarePieces.Count > 0) {
                    pieceObj = InstantiatePiece(i, j, rareObjs, Pieces.EFK, lqueRarePieces[0], dist);
                } else {
                    pieceObj = InstantiatePiece(i, j, pieceObjs, GetRandomPieceType(), Effects.NONE, dist);
                }
                pieceObj.GetComponent<Piece>().FallDistance = dist;
            }
        }
    }

    Pieces GetRandomPieceType(){
        return (Pieces)Random.Range(0, (int)Pieces.MAX);
    }

    //パフォーマンスに問題アリ。あんがいArrayCopyがきつい
    void PuzzleAI(){
        Vector2Int spieceIndex = GetSpecailPieceIndex();
        if (spieceIndex != errorIndex) {
            selectIndex = spieceIndex;
            moveDir = errorIndex;
            return;
        }

        for (int i = 0; i < HEIGHT; i++) {
            for (int j = 0; j < WIDTH; j++) {
                foreach (Direction dirValue in System.Enum.GetValues(typeof(Direction))) {
                    _field = new Pieces[HEIGHT, WIDTH];
                    System.Array.Copy(field, 0, _field, 0, field.Length);
                    Vector2Int dir = GetVector2Int(dirValue);
                    int _i = i + dir.y;
                    int _j = j + dir.x;
                    if (IsIndexOutOfRange(_i, _j)) continue;
                    Swap(ref _field[i, j], ref _field[_i, _j]);
                    checkedCell = new bool[HEIGHT, WIDTH];
                    var piece = _field[_i, _j];
                    var list = GetChainIndexs(_i, _j, piece, _field);
                    var indexs = GetDeleteIndexs(list, piece);
                    if (indexs.Count >= chainThreshold) {
                        selectIndex = new Vector2Int(j, i);
                        moveDir = dir;
                        return;
                    }
                } 
            } 
        }
        selectIndex = errorIndex;
        moveDir = errorIndex;
        Debug.Log("Not found");
    }

    Vector2Int GetSpecailPieceIndex(){
        for (int i = 0; i < HEIGHT; i++) {
            for (int j = 0; j < WIDTH; j++){
                if (fieldObjs[i, j].EfkType != Effects.NONE && fieldObjs[i, j].PieceType != Pieces.EFK) 
                    return new Vector2Int(j, i);
            }
        }
        return errorIndex;
    }
    
    //Direction -> Vector2Int
    Vector2Int GetVector2Int(Direction dir){
        switch (dir) {
            case Direction.UP   : return Vector2Int.up;
            case Direction.LEFT : return Vector2Int.left;
            case Direction.DOWN : return Vector2Int.down;
            case Direction.RIGHT: return Vector2Int.right;
        }
        return Vector2Int.zero;
    }

    IEnumerator DeleteCoroutine(){
        while (true) {
            Delete();
            if (deleteIndexs.Count > 0) {
                yield return new WaitForSeconds(deletingTime);            
                PlaySound(deleteAudio);
            }
            if (isGenerateSpecialPiece) GenerateSpecialPiece();
            Fall();
            yield return new WaitForSeconds(deletingInterval);
        }
    }

    IEnumerator AutoPuzzle(){
        SetGameStatus = GameStatus.WAIT_INTERVAL;
        PuzzleAI();
        yield return new WaitForSeconds(autoPuzzleInterval);
        if (moveDir == errorIndex) {
            if (selectIndex == errorIndex) {
                SetGameStatus = GameStatus.RESET;
            } else {
                Click();
            }
        } else {
            Swipe();
        }
    }

    IEnumerator ResetCoroutine(){
        for (int i = 0; i < HEIGHT; i++)
            for (int j = 0; j < WIDTH; j++){
                IEnumerator ie = fieldObjs[i, j].DelayDelete(resetDeletingTimeRate);
                StartCoroutine(ie);
            }

        yield return new WaitForSeconds(deletingTime * resetDeletingTimeRate);
        Init();
    }
}
