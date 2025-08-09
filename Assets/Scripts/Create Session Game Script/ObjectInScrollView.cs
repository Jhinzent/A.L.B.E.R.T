using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ObjectInScrollView : MonoBehaviour
{
    private GameObject associatedObject;
    private string text;
    private Sprite sprite;
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private Image imageComponent;
    [SerializeField] private Button buttonComponent;

    public void setAssociatedObject(GameObject newAssociatedObject) {
        associatedObject = newAssociatedObject;
    }
    public void setTextComponent(string newTextComponent) {
        text = newTextComponent;
        textComponent.text = newTextComponent;
    }
    public void setSpriteComponent(Sprite newSpriteComponent) {
        sprite = newSpriteComponent;
        imageComponent.sprite = newSpriteComponent;
    }

    public GameObject getAssociatedObject() {
        return associatedObject;
    }
    public string getText() {
        return text;
    }
    public Sprite getSprite() {
        return sprite;
    }
    public Button getButtonComponent() {
        return buttonComponent;
    }
}