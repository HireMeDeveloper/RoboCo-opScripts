using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CosmeticsManager : SingletonBehaviour<CosmeticsManager> {

    [SerializeField] private List<HeadData> headDatas = new List<HeadData>();
    [SerializeField] private List<FaceData> faceDatas = new List<FaceData>();
    [SerializeField] private List<BodyData> bodyDatas = new List<BodyData>();
    private void Start() {
        DontDestroyOnLoad(this);
    }

    public HeadData GetHead(int index) {
        return headDatas.Where((data) => data.cosmeticIndex == index).First();
    }

    public FaceData GetFace(int index) {
        return faceDatas.Where((data) => data.cosmeticIndex == index).First();
    }

    public BodyData GetBody(int index) {
        return bodyDatas.Where((data) => data.cosmeticIndex == index).First();
    }
}
