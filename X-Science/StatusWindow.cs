using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace ScienceChecklist
{
   class StatusWindow : Window<ScienceChecklistAddon>
   {
      public event EventHandler NoiseEvent;
      private readonly Texture2D _emptyTexture;
      private readonly Texture2D _progressTexture;
      private readonly Texture2D _completeTexture;
      private readonly ExperimentFilter _filter;
      private readonly Logger _logger;
      private int _previousNumExperiments;
      private float _previousUiScale;
      private GUIStyle _experimentButtonStyle;
      private GUIStyle _experimentLabelStyle;
      private GUIStyle _situationStyle;
      private GUIStyle _horizontalScrollbarOnboardStyle;
      private GUIStyle _progressLabelStyle;
      private IList<ModuleScienceExperiment> _moduleScienceExperiments;
      private Dictionary<string, bool> _availableScienceExperiments;


      public StatusWindow(ScienceChecklistAddon Parent)
         : base("[x] Science! Here and Now", 250, 30, Parent)
      {
         _logger = new Logger(this);
         _filter = new ExperimentFilter(_parent);
         _filter.DisplayMode = DisplayMode.CurrentSituation;
         _filter.EnforceLabLanderMode = true;

         _emptyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
         _emptyTexture.SetPixels(new[] { Color.clear });
         _emptyTexture.Apply();
         _progressTexture = TextureHelper.FromResource("ScienceChecklist.icons.scienceProgress.png", 13, 13);
         _completeTexture = TextureHelper.FromResource("ScienceChecklist.icons.scienceComplete.png", 13, 13);

         _availableScienceExperiments = new Dictionary<string, bool>();

         _parent.Config.HideCompleteEventsChanged += (s, e) => RefreshFilter(s, e);
         _parent.Config.CompleteWithoutRecoveryChanged += (s, e) => RefreshFilter(s, e);

         _parent.ScienceEventHandler.FilterUpdateEvent += (s, e) => RefreshFilter(s, e);
         _parent.ScienceEventHandler.SituationChanged += (s, e) => UpdateSituation(s, e);
         this.Resizable = false;
         _filter.UpdateFilter();
      }



      protected override void ConfigureStyles()
      {
         base.ConfigureStyles();

         _progressLabelStyle = new GUIStyle(_skin.label)
         {
            fontStyle = FontStyle.BoldAndItalic,
            alignment = TextAnchor.MiddleCenter,
            fontSize = wScale(11),
            normal = {
               textColor = new Color(0.322f, 0.298f, 0.004f)
            }
         };

         _horizontalScrollbarOnboardStyle = new GUIStyle(_skin.horizontalScrollbar)
         {
            normal = { background = _emptyTexture }
         };

         _situationStyle = new GUIStyle(_skin.label)
         {
            fontSize = wScale(13),
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Normal,
            fixedHeight = wScale(25),
            contentOffset = wScale(new Vector2(0, 6)),
            wordWrap = true,
            normal = {
               textColor = new Color(0.7f, 0.8f, 0.8f)
            }
         };
         _experimentButtonStyle = new GUIStyle(_skin.button)
         {
            fontSize = wScale(14)
         };
         _experimentLabelStyle = new GUIStyle(_experimentButtonStyle)
         {
            fontSize = wScale(14),
            normal = { textColor = Color.black }
         };
      }



      protected override void DrawWindowContents(int windowID)
      {
         GUILayout.BeginVertical();

         if (_filter.CurrentSituation != null && _parent.Science.CurrentVesselScience != null)
         {
            var desc = _filter.CurrentSituation.Description;
            GUILayout.Box
            (
               new GUIContent
               (
                  char.ToUpper(desc[0]) + desc.Substring(1),
                  "Current Vessel: " + _parent.Science.CurrentVesselScience.Count() + " stored experiments"
               ),
               _situationStyle,
               GUILayout.Width(wScale(250))
            );
         }
         int Top = wScale(65);
         if (_filter.DisplayScienceInstances != null && _filter.DisplayScienceInstances != null)
         {
            for (var i = 0; i < _filter.DisplayScienceInstances.Count; i++)
            {
               var rect = new Rect(wScale(5), Top, wScale(250), wScale(30));
               var experiment = _filter.DisplayScienceInstances[i];
               DrawExperiment(experiment, rect);
               Top += wScale(35);
            }
         }
         else
            _logger.Trace("DisplayExperiments is null");


         GUILayout.EndVertical();



         /*			GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
                  if( GUILayout.Button( new GUIContent( "Kill Warp", "Kill Warp" ), GUILayout.Width( 100 ), GUILayout.Height( 23 ) ) )
                  {
                     GameHelper.StopTimeWarp( );
                  }

                  if( GUILayout.Button( new GUIContent( "Play Noise", "Play Noise" ), GUILayout.Width( 100 ), GUILayout.Height( 23 ) ) )
                  {
                     PlayNoise( );
                  }
                  GUILayout.EndHorizontal( );*/

         GUILayout.Space(wScale(2));
      }



      public override void DrawWindow()
      {
         // The window needs to get smaller when the number of experiments drops.
         // This allows that while preventing flickering.
         if (_previousNumExperiments != _filter.DisplayScienceInstances.Count || _parent.Config.UiScale != _previousUiScale)
         {
            windowPos.height = wScale(30) + ((_filter.DisplayScienceInstances.Count + 1) * wScale(35));
            windowPos.width = wScale(defaultWindowSize.x);
            _previousNumExperiments = _filter.DisplayScienceInstances.Count;
            _previousUiScale = _parent.Config.UiScale;
         }

         base.DrawWindow();
      }



      private void PlayNoise()
      {
         if (NoiseEvent != null)
         {
            NoiseEvent(this, EventArgs.Empty);
         }
      }



      private void DrawExperiment(ScienceInstance exp, Rect rect)
      {
         bool ExperimentRunnable = CanRunExperiment(exp, true);
         Rect buttonRect = new Rect(rect) { xMax = wScale(200) };

         if (ExperimentRunnable)
         {
            _experimentButtonStyle.normal.textColor = exp.IsComplete ? Color.green : Color.yellow;
            if (GUI.Button(buttonRect, exp.ShortDescription, _experimentButtonStyle))
            {
               RunExperiment(exp);
            }
         }
         else
         {
            GUI.Label(buttonRect, exp.ShortDescription, _experimentLabelStyle);
         }
         int Dif = (int)(((rect.yMax - rect.yMin) - wScale(13)) / 2);
         Rect progressRect = new Rect(wScale(205), rect.yMin + Dif, wScale(50), wScale(13));
         ProgressBar(progressRect, exp.CompletedScience, exp.TotalScience, exp.CompletedScience + exp.OnboardScience);
      }



      private void ProgressBar(Rect rect, float curr, float total, float curr2)
      {
         var completeTexture = _completeTexture;
         var progressTexture = _progressTexture;
         var complete = curr > total || (total - curr < 0.1);
         if (complete)
            curr = total;
         var progressRect = new Rect(rect.xMin, rect.yMin, rect.width, wScale(13));

         GUI.skin.horizontalScrollbar.fixedHeight = wScale(13);
         GUI.skin.horizontalScrollbarThumb.fixedHeight = wScale(13);

         if (curr2 != 0 && !complete)
         {
            var complete2 = false;
            if (curr2 > total || (total - curr2 < 0.1))
            {
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

         var showValues = true;
         if (showValues)
         {
            var labelRect = new Rect(progressRect)
            {
               y = progressRect.y + 1,
            };
            GUI.Label(labelRect, string.Format("{0:0.#}  /  {1:0.#}", curr, total), _progressLabelStyle);
         }
      }



      #region Events called when science changes
      // Refreshes the experiment filter.
      // This is the lightest update used when the vessel changes
      public void RefreshFilter(object sender, EventArgs e)
      {
         //			_logger.Trace( "RefreshFilter" );

         if (_moduleScienceExperiments != null)
            _moduleScienceExperiments.Clear();
         if (_availableScienceExperiments != null)
            _availableScienceExperiments.Clear();


         Vessel v = FlightGlobals.ActiveVessel;
         if (v != null && HighLogic.LoadedScene == GameScenes.FLIGHT)
            _moduleScienceExperiments = v.FindPartModulesImplementing<ModuleScienceExperiment>();



         _filter.UpdateFilter();
      }



      // Bung new situation into filter and recalculate everything
      public void UpdateSituation(object sender, NewSituationData e)
      {
         //			_logger.Trace( "UpdateSituation" );
         _filter.CurrentSituation = new Situation(e._body, e._situation, e._biome, e._subBiome);
         RefreshFilter(sender, e);
         if (_filter.TotalCount > 0)
         {
            if (_filter.TotalCount - _filter.CompleteCount > 0)
            {
               if (IsVisible())
               {
                  if (_parent.Config.StopTimeWarp)
                     GameHelper.StopTimeWarp();
                  if (_parent.Config.PlayNoise)
                     PlayNoise();
                  if (_parent.Config.StopTimeWarp || _parent.Config.PlayNoise)
                     ScreenMessages.PostScreenMessage("New Situation: " + _filter.CurrentSituation.Description);
               }
            }
         }
      }
      #endregion



      public bool CanRunExperiment(ScienceInstance s, bool runSingleUse = true)
      {
         bool IsAvailable = false;
         if (_availableScienceExperiments.ContainsKey(s.ScienceExperiment.id))
            return _availableScienceExperiments[s.ScienceExperiment.id];
         if (_moduleScienceExperiments != null && _moduleScienceExperiments.Count > 0)
         {
            IEnumerable<ModuleScienceExperiment> lm = _moduleScienceExperiments.Where(x => (
               x.experimentID == s.ScienceExperiment.id &&
               !(x.GetScienceCount() > 0) &&
               (x.rerunnable || runSingleUse) &&
               !x.Inoperable
               ));

            IsAvailable = lm.Count() != 0;
            _availableScienceExperiments[s.ScienceExperiment.id] = IsAvailable;
         }
         return IsAvailable;
      }



      public ModuleScienceExperiment FindExperiment(ScienceInstance s, bool runSingleUse = true)
      {
         ModuleScienceExperiment m = null;
         if (_moduleScienceExperiments != null && _moduleScienceExperiments.Count > 0)
         {
            IEnumerable<ModuleScienceExperiment> lm = _moduleScienceExperiments.Where(x => (
               x.experimentID == s.ScienceExperiment.id &&
               !(x.GetScienceCount() > 0) &&
               (x.rerunnable || runSingleUse) &&
               !x.Inoperable
               ));
            if (lm.Count() != 0)
               m = lm.First();
         }
         return m;
      }



      public void RunExperiment(ScienceInstance s, bool runSingleUse = true)
      {
         ModuleScienceExperiment m = FindExperiment(s, runSingleUse);
         if (m != null)
         {
            //				_logger.Trace( "Running Experiment " + m.experimentID + " on part " + m.part.partInfo.name );
            RunStandardModuleScienceExperiment(m);
            return;
         }
      }



      public void RunStandardModuleScienceExperiment(ModuleScienceExperiment exp)
      {
         if (exp.Inoperable) return;

         if (_parent.Config.ShowResultsWindow)
            exp.DeployExperiment();
         else
         {
            if (!exp.useStaging)
            {
               exp.useStaging = true;
               exp.OnActive();
               exp.useStaging = false;
            }
            else
               exp.OnActive();
         }
      }
   }
}
