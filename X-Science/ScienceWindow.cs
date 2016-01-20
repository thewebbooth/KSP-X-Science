using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceChecklist {
	/// <summary>
	/// Renders a window containing experiments to the screen.
	/// </summary>
	internal sealed class ScienceWindow {
		/// <summary>
		/// Creates a new instance of the ScienceWindow class.
		/// </summary>
		public ScienceWindow () {
			_logger = new Logger(this);
			_rect = new Rect(40, 40, 500, 400);
			_rect3 = new Rect(40, 40, 400, 200);
			_scrollPos = new Vector2();
			_filter = new ExperimentFilter();
			_progressTexture =			TextureHelper.FromResource( "ScienceChecklist.icons.scienceProgress.png", 13, 13 );
			_completeTexture =			TextureHelper.FromResource( "ScienceChecklist.icons.scienceComplete.png", 13, 13 );
			_progressTextureCompact =	TextureHelper.FromResource( "ScienceChecklist.icons.scienceProgressCompact.png", 8, 8 );
			_completeTextureCompact =	TextureHelper.FromResource( "ScienceChecklist.icons.scienceCompleteCompact.png", 8, 8 );
			_currentSituationTexture =	TextureHelper.FromResource( "ScienceChecklist.icons.currentSituation.png", 25, 21 );
			_currentVesselTexture =		TextureHelper.FromResource( "ScienceChecklist.icons.currentVessel.png", 25, 21 );
			_unlockedTexture =			TextureHelper.FromResource( "ScienceChecklist.icons.unlocked.png", 25, 21 );
			_allTexture =				TextureHelper.FromResource( "ScienceChecklist.icons.all.png", 25, 21 );
			_searchTexture =			TextureHelper.FromResource( "ScienceChecklist.icons.search.png", 25, 21 );
			_clearSearchTexture =		TextureHelper.FromResource( "ScienceChecklist.icons.clearSearch.png", 25, 21 );
			_settingsTexture =			TextureHelper.FromResource( "ScienceChecklist.icons.settings.png", 25, 21 );
			_maximizeTexture =			TextureHelper.FromResource( "ScienceChecklist.icons.minimize.png", 25, 21 );
			_minimizeTexture =			TextureHelper.FromResource( "ScienceChecklist.icons.maximize.png", 25, 21 );

			_emptyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			_emptyTexture.SetPixels(new[] { Color.clear });
			_emptyTexture.Apply();
			_settingsPanel = new SettingsPanel();
			_settingsPanel.HideCompleteEventsChanged += (s, e) => _filter.UpdateFilter( );
			_settingsPanel.CheckDebrisChanged += ( s, e ) => _filter.UpdateExperiments( );
			_settingsPanel.CompleteWithoutRecoveryChanged += ( s, e ) => _filter.UpdateFilter( );
		}


		public event EventHandler OnCloseEvent;


		#region PROPERTIES

		/// <summary>
		/// Gets or sets a value indicating whether this window should be drawn.
		/// </summary>
		public bool IsVisible { get; set; }
		public bool UIHidden { get; set; }

		/// <summary>
		/// Gets the settings for this window.
		/// </summary>
		public SettingsPanel Settings { get { return _settingsPanel; } }

		#endregion

		#region METHODS (PUBLIC)



		/// <summary>
		/// Draws the window if it is visible.
		/// </summary>
		public void Draw( )
		{
			if( UIHidden )
			{
				return;
			} 
			
			if( !IsVisible )
			{
				return;
			}
			if( !GameHelper.WindowVisibility( ) )
			{
				IsVisible = false;
				OnCloseEvent( this, EventArgs.Empty );
			}

			if( _skin == null )
			{
				// Initialize our skin and styles.
				_skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

				_skin.horizontalScrollbarThumb.fixedHeight = 13;
				_skin.horizontalScrollbar.fixedHeight = 13;

				_labelStyle = new GUIStyle(_skin.label) {
					fontSize = 11,
					fontStyle = FontStyle.Italic,
				};

				_progressLabelStyle = new GUIStyle(_skin.label) {
					fontStyle = FontStyle.BoldAndItalic,
					alignment = TextAnchor.MiddleCenter,
					fontSize = 11,
					normal = {
						textColor = new Color(0.322f, 0.298f, 0.004f),
					},
				};

				_situationStyle = new GUIStyle(_progressLabelStyle) {
					fontSize = 13,
					alignment = TextAnchor.MiddleLeft,
					fontStyle = FontStyle.Normal,
					fixedHeight = 25,
					contentOffset = new Vector2(0, 6),
					wordWrap = true,
					normal = {
						textColor = new Color(0.7f, 0.8f, 0.8f),
					},
				};

				_experimentProgressLabelStyle = new GUIStyle(_skin.label) {
					padding = new RectOffset(0, 0, 4, 0),
				};

				_horizontalScrollbarOnboardStyle = new GUIStyle(_skin.horizontalScrollbar) {
					normal = {
						background = _emptyTexture,
					},
				};

				_compactWindowStyle = new GUIStyle(_skin.window) {
					padding = new RectOffset(0, 4, 4, 4),
				};

				_compactLabelStyle = new GUIStyle(_labelStyle) {
					fontSize = 9,
				};

				_compactSituationStyle = new GUIStyle(_situationStyle) {
					fontSize = 11,
					contentOffset = new Vector2(0, -3),
				};

				_compactButtonStyle = new GUIStyle(_skin.button) {
					padding = new RectOffset(),
					fixedHeight = 16
				};
				_closeButtonStyle = new GUIStyle( _skin.button )
				{
					// int left, int right, int top, int bottom
					padding = new RectOffset( 2, 2, 6, 2 ),
				};
			}

			var oldSkin = GUI.skin;
			GUI.skin = _skin;

			if( _compactMode )
			{
				_rect3 = GUILayout.Window(_window3Id, _rect3, DrawCompactControls, string.Empty, _compactWindowStyle);
			}
			else
			{
				_rect = GUILayout.Window( _windowId, _rect, DrawControls, "[x] Science!");
			}



			if (!string.IsNullOrEmpty(_lastTooltip)) {
				_tooltipStyle = _tooltipStyle ?? new GUIStyle(_skin.window) {
					normal = {
						background = GUI.skin.window.normal.background
					},
					wordWrap = true
				};
				/*GUI.Window(_window2Id, new Rect(Mouse.screenPos.x + 15, Mouse.screenPos.y + 15, 200, 75), x => {
					GUI.Label(new Rect(), _lastTooltip);
				}, string.Empty, _tooltipStyle);
				 
				  float oneLineHeight = _tooltipStyle.CalcHeight(new GUIContent(""), 500);
 float answerHeight = _tooltipStyle.CalcHeight(new GUIContent(answer1), 500);
				 
				 */
				//				int w = 7 * GUI.tooltip.Length;
GUI.Window(_window2Id, new Rect(Mouse.screenPos.x + 15, Mouse.screenPos.y + 15, 200, 200), x => {
 				GUI.Box( new Rect( 5, 5, 190, 190 ), _lastTooltip );
}, string.Empty, _tooltipStyle );
			}

			GUI.skin = oldSkin;
		}



		/// <summary>
		/// Refreshes the experiment cache. EXPENSIVE.
		/// </summary>
		public void RefreshExperimentCache () {
//			_logger.Trace("RefreshExperimentCache");
			_filter.RefreshExperimentCache();
		}



		/// <summary>
		/// Refreshes the experiment filter.
		/// </summary>
		public void RefreshFilter () {
//			_logger.Trace("RefreshFilter");
			_filter.UpdateFilter();
		}



		/// <summary>
		/// Updates all experiments.
		/// </summary>
		public void UpdateExperiments( )
		{
//			_logger.Trace("UpdateExperiments");
			_filter.UpdateExperiments( );
			_filter.UpdateFilter( ); // Need to do this, otherwise complete count can be incorrect after vehicle recovery.
		}



		/// <summary>
		/// Recalculates the current situation of the active vessel.
		/// </summary>
		public void RecalculateSituation () {
			var vessel = FlightGlobals.ActiveVessel;
			if (vessel == null) {
				if (_filter.CurrentSituation != null) {
					_filter.CurrentSituation = null;
				}
				return;
			}

			var body = vessel.mainBody;
			var situation = ScienceUtil.GetExperimentSituation(vessel);

			var biome = ScienceUtil.GetExperimentBiome(body, vessel.latitude, vessel.longitude);
			var subBiome = string.IsNullOrEmpty(vessel.landedAt)
				? null
				: Vessel.GetLandedAtString(vessel.landedAt).Replace(" ", "");

			var dataCount = vessel.FindPartModulesImplementing<IScienceDataContainer>().Sum(x => x.GetData().Length);

			if (_lastDataCount != dataCount) {
				_lastDataCount = dataCount;
				UpdateExperiments();
			}

			if (_filter.CurrentSituation != null && _filter.CurrentSituation.Biome == biome && _filter.CurrentSituation.ExperimentSituation == situation && _filter.CurrentSituation.Body.CelestialBody == body && _filter.CurrentSituation.SubBiome == subBiome) {
				return;
			}
			var Body = new Body( body );
			_filter.CurrentSituation = new Situation(Body, situation, biome, subBiome);
		}



		#endregion

		#region METHODS (PRIVATE)

		/// <summary>
		/// Draws the controls inside the window.
		/// </summary>
		/// <param name="windowId"></param>
		private void DrawControls (int windowId) {

			if( GUI.Button( new Rect( _rect.width - 20, 4, 18, 18 ), new GUIContent( "X", "Close [x] Science" ), _closeButtonStyle ) )
			{
				IsVisible = false;
				OnCloseEvent( this, EventArgs.Empty );
			}
			GUILayout.BeginHorizontal ();

			GUILayout.BeginVertical(GUILayout.Width(480), GUILayout.ExpandHeight(true));

			ProgressBar(
				new Rect (10, 27, 480, 13),
				_filter.TotalCount == 0 ? 1 : _filter.CompleteCount,
				_filter.TotalCount == 0 ? 1 : _filter.TotalCount,
				0,
				false,
				false);

			GUILayout.Space(20);

			GUILayout.BeginHorizontal();

			GUILayout.Label(string.Format("{0}/{1} complete.", _filter.CompleteCount, _filter.TotalCount), _experimentProgressLabelStyle, GUILayout.Width(150));
			GUILayout.FlexibleSpace();
			GUILayout.Label(new GUIContent(_searchTexture));
			_filter.Text = GUILayout.TextField(_filter.Text, GUILayout.Width(150));

			if (GUILayout.Button(new GUIContent(_clearSearchTexture, "Clear search"), GUILayout.Width(25), GUILayout.Height(23))) {
				_filter.Text = string.Empty;
			}

			GUILayout.EndHorizontal();

			_scrollPos = GUILayout.BeginScrollView(_scrollPos, _skin.scrollView);
			var i = 0;
			if( _filter.DisplayScienceInstances == null )
				_logger.Trace( "DisplayExperiments is null" );
			else
			{



				for( ; i < _filter.DisplayScienceInstances.Count; i++ )
				{
					var rect = new Rect( 5, 20 * i, _filter.DisplayScienceInstances.Count > 13 ? 490 : 500, 20 );
				if (rect.yMax < _scrollPos.y || rect.yMin > _scrollPos.y + 400) {
					continue;
				}

				var experiment = _filter.DisplayScienceInstances[ i ];
				DrawExperiment(experiment, rect, false, _labelStyle);
			}
			}

			GUILayout.Space(20 * i);
			GUILayout.EndScrollView();
			
			GUILayout.BeginHorizontal();


			GUIContent[ ] FilterButtons = {
					new GUIContent(_currentSituationTexture, "Show experiments available right now"),
					new GUIContent(_currentVesselTexture, "Show experiments available on this vessel"),
					new GUIContent(_unlockedTexture, "Show all unlocked experiments")
				};
			if( Config.AllFilter )
			{
				Array.Resize( ref FilterButtons, 4 );
				FilterButtons[ 3 ] = new GUIContent(_allTexture, "Show all experiments");
			}

			_filter.DisplayMode = (DisplayMode)GUILayout.SelectionGrid( (int)_filter.DisplayMode, FilterButtons, 4 );

			GUILayout.FlexibleSpace();

			if (_filter.CurrentSituation != null) {
				var desc = _filter.CurrentSituation.Description;
				GUILayout.Box(char.ToUpper(desc[0]) + desc.Substring(1), _situationStyle);
			}

			GUILayout.FlexibleSpace();
			
			var toggleSettings = GUILayout.Button(new GUIContent(_settingsTexture, "Settings"));
			if (toggleSettings) {
				_showSettings = !_showSettings;
				_rect.width = _showSettings ? 800 : 500;
			}

			var toggleCompact = GUILayout.Button(new GUIContent(_minimizeTexture, "Compact mode"));
			if (toggleCompact) {
				_compactMode = !_compactMode;
			}
			
			GUILayout.EndHorizontal();
			GUILayout.EndVertical ();

			if (_showSettings) {
				_settingsPanel.Draw();
			}

			GUILayout.EndHorizontal ();
			GUI.DragWindow();

			if (Event.current.type == EventType.Repaint && GUI.tooltip != _lastTooltip) {
				_lastTooltip = GUI.tooltip;
			}
			
			// If this window gets focus, it pushes the tooltip behind the window, which looks weird.
			// Just hide the tooltip while mouse buttons are held down to avoid this.
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
				_lastTooltip = string.Empty;
			}
		}



		/// <summary>
		/// Draws the controls for the window in compact mode.
		/// </summary>
		/// <param name="windowId"></param>
		private void DrawCompactControls (int windowId) {
			GUILayout.BeginVertical();
			_compactScrollPos = GUILayout.BeginScrollView(_compactScrollPos);
			var i = 0;
			if( _filter.DisplayScienceInstances != null )
			{
				for( ; i < _filter.DisplayScienceInstances.Count; i++ )
				{

					var rect = new Rect( 5, 15 * i, _filter.DisplayScienceInstances.Count > 11 ? 405 : 420, 20 );
					if (rect.yMax < _compactScrollPos.y || rect.yMin > _compactScrollPos.y + 400) {
						continue;
					}

					var experiment = _filter.DisplayScienceInstances[ i ];
					DrawExperiment(experiment, rect, true, _compactLabelStyle);
				}
			}
			else
				_logger.Trace( "DisplayExperiments is null" );
			GUILayout.Space(15 * i);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.BeginHorizontal();



			var CloseButton = GUILayout.Button( "X", _compactButtonStyle, GUILayout.Height( 16 ), GUILayout.Width( 16 ) );
			if( CloseButton )
			{
				IsVisible = false;
				OnCloseEvent( this, EventArgs.Empty );
			}


			GUILayout.FlexibleSpace();
			if (_filter.CurrentSituation != null) {
				var desc = _filter.CurrentSituation.Description;
				GUILayout.Label(char.ToUpper(desc[0]) + desc.Substring(1), _compactSituationStyle, GUILayout.Height(16));
			}



			GUILayout.FlexibleSpace();

			var toggleCompact = GUILayout.Button(new GUIContent(_maximizeTexture, "Normal mode"), _compactButtonStyle, GUILayout.Height(16), GUILayout.Width(16));
			if (toggleCompact) {
				_compactMode = !_compactMode;
			}
			
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}

		/// <summary>
		/// Draws an experiment inside the given Rect.
		/// </summary>
		/// <param name="exp">The experiment to render.</param>
		/// <param name="rect">The rect inside which the experiment should be rendered.</param>
		/// <param name="compact">Whether this experiment is compact.</param>
		/// <param name="labelStyle">The style to use for labels.</param>
		private void DrawExperiment (ScienceInstance exp, Rect rect, bool compact, GUIStyle labelStyle) {
			labelStyle.normal.textColor = exp.IsComplete ? Color.green : Color.yellow;
			var labelRect = new Rect(rect) {
				y = rect.y + (compact ? 1 : 3),
			};
			var progressRect = new Rect(rect) {
				xMin = rect.xMax - (compact ? 75 : 105),
				xMax = rect.xMax - (compact ? 40 : 40),
				y = rect.y + (compact ? 1 : 3),
			};

			GUI.Label(labelRect, exp.Description, labelStyle);
			GUI.skin.horizontalScrollbar.fixedHeight = compact ? 8 : 13;
			GUI.skin.horizontalScrollbarThumb.fixedHeight = compact ? 8 : 13;
			ProgressBar(progressRect, exp.CompletedScience, exp.TotalScience, exp.CompletedScience + exp.OnboardScience, !compact, compact);
		}

		/// <summary>
		/// Draws a progress bar inside the given Rect.
		/// </summary>
		/// <param name="rect">The rect inside which the progress bar should be rendered.</param>
		/// <param name="curr">The completed progress value.</param>
		/// <param name="total">The total progress value.</param>
		/// <param name="curr2">The shaded progress value (used to show onboard science).</param>
		/// <param name="showValues">Whether to draw the curr and total values on top of the progress bar.</param>
		/// <param name="compact">Whether this progress bar should be rendered in compact mode.</param>
		private void ProgressBar (Rect rect, float curr, float total, float curr2, bool showValues, bool compact) {
			var completeTexture = compact ? _completeTextureCompact : _completeTexture;
			var progressTexture = compact ? _progressTextureCompact : _progressTexture;
			var complete = curr > total || (total - curr < 0.1);
			if (complete) {
				curr = total;
			}
			var progressRect = new Rect(rect) {
				y = rect.y + (compact ? 3 : 1),
			};

			if (curr2 != 0 && !complete) {
				var complete2 = false;
				if (curr2 > total || (total - curr2 < 0.1)) {
					curr2 = total;
					complete2 = true;
				}
				_skin.horizontalScrollbarThumb.normal.background = curr2 < 0.1
					? _emptyTexture
					: complete2
						? completeTexture
						: progressTexture;

				GUI.HorizontalScrollbar(progressRect, 0, curr2 / total, 0, 1, _horizontalScrollbarOnboardStyle);
			}

			_skin.horizontalScrollbarThumb.normal.background = curr < 0.1
				? _emptyTexture
				: complete ? completeTexture : progressTexture;

			GUI.HorizontalScrollbar(progressRect, 0, curr / total, 0, 1);

			if (showValues) {
				var labelRect = new Rect(rect) {
					y = rect.y - 1,
				};
				GUI.Label(labelRect, string.Format("{0:0.#}  /  {1:0.#}", curr, total), _progressLabelStyle);
			}
		}

		#endregion

		#region FIELDS

		private Rect _rect;
		private Rect _rect3;
		private Vector2 _scrollPos;
		private Vector2 _compactScrollPos;
		private GUIStyle _labelStyle;
		private GUIStyle _horizontalScrollbarOnboardStyle;
		private GUIStyle _progressLabelStyle;
		private GUIStyle _situationStyle;
		private GUIStyle _experimentProgressLabelStyle;
		private GUIStyle _tooltipStyle;
		private GUIStyle _compactWindowStyle;
		private GUIStyle _compactLabelStyle;
		private GUIStyle _compactSituationStyle;
		private GUIStyle _compactButtonStyle;
		private GUIStyle _closeButtonStyle;
		private GUISkin _skin;

		private string _lastTooltip;
		private bool _showSettings;
		private int _lastDataCount;
		private bool _compactMode;

		private readonly Texture2D _progressTexture;
		private readonly Texture2D _completeTexture;
		private readonly Texture2D _progressTextureCompact;
		private readonly Texture2D _completeTextureCompact;
		private readonly Texture2D _emptyTexture;
		private readonly Texture2D _currentSituationTexture;
		private readonly Texture2D _currentVesselTexture;
		private readonly Texture2D _unlockedTexture;
		private readonly Texture2D _allTexture;
		private readonly Texture2D _searchTexture;
		private readonly Texture2D _clearSearchTexture;
		private readonly Texture2D _settingsTexture;
		private readonly Texture2D _minimizeTexture;
		private readonly Texture2D _maximizeTexture;
		private readonly SettingsPanel _settingsPanel;

		private readonly ExperimentFilter _filter;
		private readonly Logger _logger;
		private readonly int _windowId = UnityEngine.Random.Range(0, int.MaxValue);
		private readonly int _window2Id = UnityEngine.Random.Range(0, int.MaxValue);
		private readonly int _window3Id = UnityEngine.Random.Range(0, int.MaxValue);

		#endregion
	}
}
