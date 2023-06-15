using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VRTown.Game;
using VRTown.Model;

public class UICharacter : MonoBehaviour
{
    [SerializeField] RuntimeAnimatorController _maleAnimator;
    [SerializeField] RuntimeAnimatorController _femaleAnimator;
    [SerializeField] RuntimeAnimatorController _animeAnimator;
    IModelController _modelController = null;
    GModel _modelCharacter;

    private Vector2 lastDragPosition;
    private Vector3 startDragRotation;
    private Quaternion targetRotation;
    [SerializeField] GameObject rotateDegreeText;

    Dictionary<GenderType, RuntimeAnimatorController> _animators = new Dictionary<GenderType, RuntimeAnimatorController>();

    public void Create(IModelController modelController, UserData userData)
    {
        _animators.Clear();
        _animators.Add(GenderType.male, _maleAnimator);
        _animators.Add(GenderType.female, _femaleAnimator);
        _animators.Add(GenderType.anime, _animeAnimator);

        _modelController = modelController;
        _modelCharacter = _modelController.CreateCharacter(userData);
        _modelCharacter.transform.SetParent(this.transform);
        _modelCharacter.transform.ResetTransform();

        RunIdle();
        RefreshView();
        UpdateUIRotate(0);
    }
    
    public void ChangeSkin(UserData newData)
    {
        _modelController.ChangeSkin(ref _modelCharacter, newData);
        RunIdle();
        RefreshView();
    }

    public void ChangeSkinColor(int newColorId)
    {
        _modelController.ChangeSkinColor(_modelCharacter, newColorId);
    }

    public void CreateProp(PropData dataProp, int colorId = -1)
    {
        _modelController.CreateProp(_modelCharacter, dataProp, colorId);
    }

    void RefreshView()
    {
        _modelCharacter.transform.localScale = Vector3.one * 200f;
        _modelCharacter.transform.localPosition = new Vector3(0, 0, -500f);
        _modelCharacter.transform.localEulerAngles = new Vector3(0, 180f, 0);
    }

    void RunIdle()
    {
        _modelCharacter.gameObject.setLayerRecursively(LayerMask.NameToLayer("UI"));
        var animator = _modelCharacter.GetComponentInChildren<Animator>();
        animator.runtimeAnimatorController = _animators[_modelCharacter.Gender];
    }

    public void ClearProps()
    {
        _modelController.ClearProps(_modelCharacter);
    }

    void UpdateUIRotate(int degree)
    {
        rotateDegreeText.GetComponent<TextMeshProUGUI>().text = string.Format(GHelper.Localization.Localize<string>("TXT_ROTATION") + ": " + (int)degree);
        rotateDegreeText.GetComponent<LocalizeText>().KeyLocalization = $"TXT_ROTATION";
    }

    public void OnDrag()
    {
        var dx = (Mouse.current.position.ReadValue() - lastDragPosition).x;
        targetRotation = Quaternion.Euler(startDragRotation.x, startDragRotation.y + 1.5f * dx, startDragRotation.z);
        _modelCharacter.transform.localRotation = targetRotation;
        var degree = Math.Abs(_modelCharacter.transform.localRotation.eulerAngles.y > 0 ? _modelCharacter.transform.localRotation.eulerAngles.y - 180 : _modelCharacter.transform.localRotation.eulerAngles.y + 180);
        UpdateUIRotate((int)degree);
    }

    public void OnBeginDrag()
    {
        Debug.Log("On Begin Drag");
        lastDragPosition = Mouse.current.position.ReadValue();
    }

    public void EndDrag()
    {
        Debug.Log("On End Drag");
        startDragRotation = new Vector3(_modelCharacter.transform.localRotation.x, _modelCharacter.transform.localRotation.y, _modelCharacter.transform.localRotation.z);
    }

}
