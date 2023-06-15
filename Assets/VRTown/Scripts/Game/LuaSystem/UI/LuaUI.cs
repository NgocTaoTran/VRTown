using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Scripting;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VRTown.Game.LuaSystem
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        public Button button;
        public TextMeshProUGUI textView;
        public void setData(string text, UnityAction action)
        {
            textView.text = text;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }

    public class UIPopup : MonoBehaviour
    {
        public TextMeshProUGUI textHeader;
        public TextMeshProUGUI textMessage;
        public UIButton btnConfirm;
        public UIButton btnCancel;

        public void setData(string header, string message, string textBtnConfirm, UnityAction<UIPopup> actionConfirm, string textBtnCancel, UnityAction<UIPopup> actionCancel)
        {
            textHeader.text = header;
            textMessage.text = message;
            btnConfirm.setData(textBtnConfirm, () => actionConfirm(this));
            btnCancel.setData(textBtnCancel, () => actionCancel(this));
        }
    }

    public class LuaUIView : MonoBehaviour
    {
        public override string ToString()
        {
            if (gameObject == null) return base.ToString();
            return $"LuaUIView [{gameObject.name}]";
        }

        public virtual void show(bool show)
        {
            gameObject.SetActive(show);
        }

        public virtual void destroy()
        {
            GameObject.Destroy(gameObject);
        }
    }

    public class LuaUIButton : LuaUIView
    {
    }
    public class LuaUIText : LuaUIView
    {
    }

    public class LuaUIContainer : LuaUIView
    {
        // public gameobject menubackground;
        public static GameObject buttonTemplate;
        public static GameObject itemTemplate;
        public static GameObject popupTemplate;

        // start is called before the first frame update
        void Start()
        {
        }

        private GameObject getButtonTemplate()
        {
            if (buttonTemplate == null)
            {
                var uiCommand = GameObject.Find("UICommand");
                buttonTemplate = uiCommand.transform.Find("Button").gameObject;
            }
            return buttonTemplate;
        }
        private GameObject getPopupTemplate()
        {
            if (popupTemplate == null)
            {
                var uiCommand = GameObject.Find("UICommand");
                popupTemplate = uiCommand.transform.Find("Popup").gameObject;
            }
            return popupTemplate;
        }

        private GameObject getItemTemplate()
        {
            if (itemTemplate == null)
            {
                var uiCommand = GameObject.Find("UICommand");
                itemTemplate = uiCommand.transform.Find("Item").gameObject;
            }
            return itemTemplate;
        }

        public LuaUIContainer container()
        {
            var gameObject = new GameObject("container", typeof(RectTransform), typeof(LuaUIContainer));
            var rect = gameObject.GetComponent<RectTransform>();
            gameObject.transform.SetParent(this.gameObject.transform);
            rect.anchorMin.Set(0.5f, 0.5f);
            rect.anchorMax.Set(0.5f, 0.5f);
            rect.anchoredPosition.Set(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            return gameObject.GetComponent<LuaUIContainer>();
        }
        public LuaUIContainer popup()
        {
            var template = GameObject.Instantiate(getPopupTemplate(), this.transform);
            template.AddComponent<LuaUIContainer>();
            template.SetActive(true);
            return template.GetComponent<LuaUIContainer>();
        }

        public LuaUIContainer size(string horizontal, string vertical)
        {
            var size = gameObject.AddComponent<ContentSizeFitter>();
            size.horizontalFit = horizontal == "min"
                    ? ContentSizeFitter.FitMode.MinSize
                    : horizontal == "fit"
                        ? ContentSizeFitter.FitMode.PreferredSize
                        : ContentSizeFitter.FitMode.Unconstrained;
            size.verticalFit = vertical == "min"
                    ? ContentSizeFitter.FitMode.MinSize
                    : vertical == "fit"
                        ? ContentSizeFitter.FitMode.PreferredSize
                        : ContentSizeFitter.FitMode.Unconstrained;
            return this;
        }
        public LuaUIButton button(string title, DynValue onClick)
        {
            var template = GameObject.Instantiate(getButtonTemplate(), this.transform);
            template.SetActive(true);
            template.transform.localPosition = Vector3.zero;
            var textObject = template.transform.Find("Text (TMP)");
            if (textObject != null)
            {
                var text = textObject.gameObject.GetComponent<TextMeshProUGUI>();
                text.text = title;
            }

            var button = template.GetComponent<Button>();
            if (onClick.Function != null)
            {
                button.onClick.AddListener(() =>
                {
                    LuaInterface.script.Call(onClick.Function);
                });
            }
            return template.AddComponent<LuaUIButton>();
        }

        public LuaUIButton item(string title, DynValue onClick)
        {
            var template = GameObject.Instantiate(getItemTemplate(), this.transform);
            template.SetActive(true);
            template.transform.localPosition = Vector3.zero;
            var textObject = template.transform.Find("Text (TMP)");
            if (textObject != null)
            {
                var text = textObject.gameObject.GetComponent<TextMeshProUGUI>();
                text.text = title;
            }

            var button = template.GetComponent<Button>();
            if (onClick.Function != null)
            {
                button.onClick.AddListener(() =>
                {
                    LuaInterface.script.Call(onClick.Function);
                });
            }
            return template.AddComponent<LuaUIButton>();
        }
        public LuaUIText text(string text)
        {
            var template = new GameObject("text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LuaUIText));
            template.transform.SetParent(gameObject.transform);
            template.transform.localPosition = Vector3.zero;

            var font = getItemTemplate().GetComponentInChildren<TextMeshProUGUI>().font;

            var rect = template.GetComponent<RectTransform>();
            rect.rect.Set(0, 0, 400, 50);

            var textObject = template.gameObject.GetComponent<TextMeshProUGUI>();
            textObject.enableWordWrapping = true;
            textObject.fontSize = 16;
            textObject.text = text;
            textObject.font = font;

            return template.GetComponent<LuaUIText>();
        }

        public LuaUIContainer layout(string type, int spacing = 15, int padding = 20)
        {
            HorizontalOrVerticalLayoutGroup layout;
            switch (type)
            {
                case "vert":
                    layout = gameObject.AddComponent<VerticalLayoutGroup>();
                    break;
                default:
                    layout = gameObject.AddComponent<HorizontalLayoutGroup>();
                    break;
            }
            layout.spacing = spacing;
            layout.padding.left = padding;
            layout.padding.top = padding;
            layout.padding.right = padding;
            layout.padding.bottom = padding;
            return this;
        }

        public LuaUIContainer children(List<LuaUIView> views)
        {
            return this;
        }
    }

    public class LuaUI
    {
        public LuaUI()
        {
        }

        static LuaUIContainer _root;
        public LuaUIContainer container()
        {
            if (_root == null)
            {
                var gameObject = GameObject.Find("UICommand");
                _root = gameObject.AddComponent<LuaUIContainer>();
            }
            var container = new GameObject("root", typeof(RectTransform), typeof(Image), typeof(CanvasRenderer), typeof(MeshCollider), typeof(LuaUIContainer));
            container.transform.SetParent(_root.transform);
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin.Set(0.5f, 0.5f);
            rect.anchorMax.Set(0.5f, 0.5f);
            rect.anchoredPosition.Set(0.5f, 0.5f);
            rect.rect.Set(0, 0, 600, 600);
            rect.localPosition = Vector3.zero;

            var image = container.GetComponent<Image>();
            image.color = new Color(1, 1, 1, 0);
            image.rectTransform.sizeDelta.Set(400, 400);

            return container.GetComponent<LuaUIContainer>();
        }

    }
}