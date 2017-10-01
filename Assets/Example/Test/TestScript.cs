using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TestScript : MonoBehaviour
{

    public MaskImage maskImage;

    public Button imageButton;
    public Button imageAttenButton;
    public Button rectButton;
    public Button followImageButton;
    public Button followRectButton;
    public Button gameobjectButton;
    public Button clearButton;

    public Image image0;
    public Image image1;

    public Image image2;
    public Image image3;

    public GameObject worldGameObject;

    public RectTransform rectTrans;

    public RectTransform tweenRectTrans;

    public bool m_IsTweening;

	void Start ()
	{
	    imageButton.onClick.AddListener(OnClickImageButton);
        imageAttenButton.onClick.AddListener(OnClickImageAttenButton);
        rectButton.onClick.AddListener(OnClickRectButton);
        followImageButton.onClick.AddListener(OnClickFollowImageButton);
        followRectButton.onClick.AddListener(OnClickFollowRectButton);
	    gameobjectButton.onClick.AddListener(OnClickFollowGameObject);
        clearButton.onClick.AddListener(OnClickClearButton);
    }

    void OnClickImageButton()
    {
        maskImage.ShowMask(new Color(0, 0, 0, 0.6f), 1, image0);
    }
    void OnClickImageAttenButton()
    {
        maskImage.ShowMask(new Color(0, 0, 0, 0.6f), 1f, image1.rectTransform);
        maskImage.material.SetVector("internalClipAtten", new Vector2(0.5f, 1));
        //maskImage.material.DOVector(new Vector4(0.6f, 1,0,0), "internalClipAtten", 2f);
    }
    void OnClickRectButton()
    {
        maskImage.ShowMask(new Color(0, 0, 0, 0.6f), 0.97f, rectTrans);
    }
    void OnClickFollowImageButton()
    {
        maskImage.SetFollowMask(new Color(0, 0, 0, 0.6f), 0.7f, image0);
    }
    void OnClickFollowRectButton()
    {
        maskImage.SetFollowMask(new Color(0, 0, 0, 0.6f), 1, rectTrans);
    }
    void OnClickFollowGameObject()
    {
        maskImage.ShowMaskToWorldTarget(new Color(0, 0, 0, 0.6f), 1, worldGameObject.transform, new Vector2(100, 100));
    }
    void OnClickClearButton()
    {
        maskImage.ClearMask();
    }
}
