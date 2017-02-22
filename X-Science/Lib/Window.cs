/**
 * Windows.cs
 * 
 * Thunder Aerospace Corporation's library for the Kerbal Space Program, by Taranis Elsu
 * 
 * (C) Copyright 2013, Taranis Elsu
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */
using System;
using UnityEngine;
using KSP.IO;
using KSP.UI.Dialogs;



namespace ScienceChecklist
{
   public abstract class Window<T>
   {
      private string windowTitle;
      private int windowId;
      private readonly int _tooltipWindowId = UnityEngine.Random.Range(0, int.MaxValue);
      private string configNodeName;
      protected Rect windowPos;
      protected Vector2 defaultWindowSize;
      private bool mouseDown;
      private bool visible;
      private readonly Logger _logger;
      protected string _lastTooltip;
      protected readonly ScienceChecklistAddon _parent;

      protected GUISkin _skin;
      protected GUIStyle closeButtonStyle;
      protected GUIContent closeContent;
      private GUIStyle resizeStyle;
      private GUIContent resizeContent;
      private GUIStyle _tooltipStyle;
      private GUIStyle _tooltipBoxStyle;
      private GUIStyle windowStyle;

      public bool Resizable { get; set; }
      public bool HideCloseButton { get; set; }
      public bool HideWhenPaused { get; set; }
      public event EventHandler WindowClosed;



      protected Window(string windowTitle, float defaultWidth, float defaultHeight, ScienceChecklistAddon Parent)
      {
         _parent = Parent;
         _logger = new Logger(this);

         defaultWindowSize = new Vector2(defaultWidth, defaultHeight);

         this.windowTitle = windowTitle;
         this.windowId = windowTitle.GetHashCode() + new System.Random().Next(65536);

         configNodeName = windowTitle.Replace(" ", "");

         windowPos = new Rect((Screen.width - wScale(defaultWindowSize.x)) / 2, (Screen.height - wScale(defaultWindowSize.y)) / 2, wScale(defaultWindowSize.x), wScale(defaultWindowSize.y));
         mouseDown = false;
         visible = false;

         var texture = TextureHelper.FromResource("ScienceChecklist.icons.resize.png", 16, 16);
         resizeContent = (texture != null) ? new GUIContent(texture, "Drag to resize the window") : new GUIContent("R", "Drag to resize the window");

         var closetexture = TextureHelper.FromResource("ScienceChecklist.icons.close.png", 16, 16);
         closeContent = (closetexture != null) ? new GUIContent(closetexture, "Close window") : new GUIContent("X", "Close window");


         Resizable = true;
         HideCloseButton = false;
         HideWhenPaused = true;

         _parent.Config.UiScaleChanged += OnUiScaleChange;
      }

      public bool IsVisible()
      {
         return visible;
      }

      public virtual void SetVisible(bool newValue)
      {
         this.visible = newValue;
      }

      public void ToggleVisible()
      {
         SetVisible(!visible);
      }

      public bool Contains(Vector2 point)
      {
         return windowPos.Contains(point);
      }

      public void SetSize(int width, int height)
      {
         windowPos.width = width;
         windowPos.height = height;
      }

      /*       public virtual ConfigNode Load(ConfigNode config)
				 {
					  if (config.HasNode(configNodeName))
					  {
							ConfigNode windowConfig = config.GetNode(configNodeName);

							windowPos.x = Utilities.GetValue(windowConfig, "x", windowPos.x);
							windowPos.y = Utilities.GetValue(windowConfig, "y", windowPos.y);
							windowPos.width = Utilities.GetValue(windowConfig, "width", windowPos.width);
							windowPos.height = Utilities.GetValue(windowConfig, "height", windowPos.height);

							bool newValue = Utilities.GetValue(windowConfig, "visible", visible);
							SetVisible(newValue);

							return windowConfig;
					  }
					  else
					  {
							return null;
					  }
				 }

				 public virtual ConfigNode Save(ConfigNode config)
				 {
					  ConfigNode windowConfig;
					  if (config.HasNode(configNodeName))
					  {
							windowConfig = config.GetNode(configNodeName);
							windowConfig.ClearData();
					  }
					  else
					  {
							windowConfig = config.AddNode(configNodeName);
					  }

					  windowConfig.AddValue("visible", visible);
					  windowConfig.AddValue("x", windowPos.x);
					  windowConfig.AddValue("y", windowPos.y);
					  windowConfig.AddValue("width", windowPos.width);
					  windowConfig.AddValue("height", windowPos.height);
					  return windowConfig;
				 }*/

      public virtual void DrawWindow()
      {
         if (visible)
         {
            bool paused = false;
            if (HideWhenPaused && HighLogic.LoadedSceneIsFlight)
            {
               try
               {
                  paused = PauseMenu.isOpen || FlightResultsDialog.isDisplaying;
               }
               catch (Exception)
               {
                  // ignore the error and assume the pause menu is not open
               }
            }

            if (!paused)
            {
               ConfigureStyles();
               var oldSkin = GUI.skin;
               GUI.skin = _skin;

               windowPos = Utilities.EnsureVisible(windowPos);
               windowPos = GUILayout.Window(windowId, windowPos, PreDrawWindowContents, windowTitle, GUILayout.ExpandWidth(true),
                   GUILayout.ExpandHeight(true), GUILayout.MinWidth(wScale(64)), GUILayout.MinHeight(wScale(64)));


               if (!string.IsNullOrEmpty(_lastTooltip))
               {
                  _tooltipStyle = _tooltipStyle ?? new GUIStyle(GUI.skin.window)
                  {
                     fontSize = wScale(11),
                     normal =
                     {
                        background = GUI.skin.window.normal.background
                     },
                     wordWrap = true
                  };

                  _tooltipBoxStyle = _tooltipBoxStyle ?? new GUIStyle(GUI.skin.box)
                  {
                     // int left, int right, int top, int bottom
                     fontSize = wScale(11),
                     padding = wScale(new RectOffset(4, 4, 4, 4)),
                     wordWrap = true
                  };

                  float boxHeight = _tooltipBoxStyle.CalcHeight(new GUIContent(_lastTooltip), wScale(190));
                  GUI.Window(_tooltipWindowId, new Rect(Mouse.screenPos.x + wScale(15), Mouse.screenPos.y + wScale(15), wScale(200), boxHeight + wScale(10)), x =>
                {
                   GUI.Box(new Rect(wScale(5), wScale(5), wScale(190), boxHeight), _lastTooltip, _tooltipBoxStyle);
                }, string.Empty, _tooltipStyle);
               }



               GUI.skin = oldSkin;
            }
         }
      }

      protected virtual void ConfigureStyles()
      {
         if (_skin == null)
         {
            // Initialize our skin and styles.
            _skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

            if (windowStyle == null)
            {
               windowStyle = new GUIStyle(_skin.window);
               windowStyle.fontSize = (int)(_skin.window.fontSize * _parent.Config.UiScale);
               windowStyle.padding = wScale(_skin.window.padding);
               windowStyle.margin = wScale(_skin.window.margin);
               windowStyle.border = wScale(_skin.window.border);
               windowStyle.contentOffset = wScale(_skin.window.contentOffset);
            }
            _skin.window = windowStyle;

            if (closeButtonStyle == null)
            {
               closeButtonStyle = new GUIStyle(_skin.button);
               closeButtonStyle.padding = wScale(new RectOffset(2, 2, 2, 2));
               closeButtonStyle.margin = wScale(new RectOffset(1, 1, 1, 1));
               closeButtonStyle.stretchWidth = false;
               closeButtonStyle.stretchHeight = false;
               closeButtonStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (resizeStyle == null)
            {
               resizeStyle = new GUIStyle(_skin.label);
               resizeStyle.alignment = TextAnchor.MiddleCenter;
               resizeStyle.padding = wScale(new RectOffset(1, 1, 1, 1));
            }
         }
      }



      private void PreDrawWindowContents(int windowId)
      {
         DrawWindowContents(windowId);

         if (!HideCloseButton)
         {
            if (GUI.Button(wScale(new Rect(4, 4, 20, 20)), closeContent, closeButtonStyle))
            {
               SetVisible(false);
               OnClose(EventArgs.Empty);
            }
         }

         if (Resizable)
         {
            var resizeRect = new Rect(windowPos.width - wScale(16), windowPos.height - wScale(16), wScale(16), wScale(16));
            GUI.Label(resizeRect, resizeContent, resizeStyle);

            HandleWindowEvents(resizeRect);
         }
         if (Event.current.type == EventType.Repaint && GUI.tooltip != _lastTooltip)
         {
            _lastTooltip = GUI.tooltip;
         }

         // If this window gets focus, it pushes the tooltip behind the window, which looks weird.
         // Just hide the tooltip while mouse buttons are held down to avoid this.
         if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
         {
            _lastTooltip = string.Empty;
         }
         GUI.DragWindow();
      }

      protected abstract void DrawWindowContents(int windowId);



      private void HandleWindowEvents(Rect resizeRect)
      {
         var theEvent = Event.current;
         if (theEvent != null)
         {
            if (!mouseDown)
            {
               if (theEvent.type == EventType.MouseDown && theEvent.button == 0 && resizeRect.Contains(theEvent.mousePosition))
               {
                  mouseDown = true;
                  theEvent.Use();
               }
            }
            else if (theEvent.type != EventType.Layout)
            {
               if (Input.GetMouseButton(0))
               {
                  // Flip the mouse Y so that 0 is at the top
                  float mouseY = Screen.height - Input.mousePosition.y;

                  windowPos.width = Mathf.Clamp(Input.mousePosition.x - windowPos.x + (resizeRect.width / 2), wScale(50), Screen.width - windowPos.x);
                  windowPos.height = Mathf.Clamp(mouseY - windowPos.y + (resizeRect.height / 2), wScale(50), Screen.height - windowPos.y);
               }
               else
               {
                  mouseDown = false;
               }
            }
         }
      }
      private void OnClose(EventArgs e)
      {
         //			_logger.Trace( "Window Closed" );
         if (WindowClosed != null)
            WindowClosed(this, e);
      }

      protected virtual void OnUiScaleChange(object sender, EventArgs e)
      {
         _skin = null;
         closeButtonStyle = null;
         resizeStyle = null;
         _tooltipStyle = null;
         _tooltipBoxStyle = null;
         windowStyle = null;

         ConfigureStyles();
      }

      protected int wScale(int v) { return Convert.ToInt32(Math.Round(v * _parent.Config.UiScale)); }
      protected float wScale(float v) { return v * _parent.Config.UiScale; }
      protected Rect wScale(Rect v)
      {
         return new Rect(wScale(v.x), wScale(v.y), wScale(v.width), wScale(v.height));
      }
      protected RectOffset wScale(RectOffset v)
      {
         return new RectOffset(wScale(v.left), wScale(v.right), wScale(v.top), wScale(v.bottom));
      }
      protected Vector2 wScale(Vector2 v)
      {
         return new Vector2(wScale(v.x), wScale(v.y));
      }

   }

}
