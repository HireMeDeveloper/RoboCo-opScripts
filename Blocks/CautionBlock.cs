using UnityEngine;

public class CautionBlock : Block {
    public BlockColor color;

    [Space]
    [SerializeField] private GameObject activeBlock;
    [SerializeField] private GameObject inactiveBlock;

    public void SetIsABlock(bool isABlock) {
        if (isABlock) {
            activeBlock.SetActive(true);
            inactiveBlock.SetActive(false);
        } else {
            activeBlock.SetActive(false);
            inactiveBlock.SetActive(true);
        }
    }
}
