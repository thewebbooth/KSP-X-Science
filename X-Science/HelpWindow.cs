

using UnityEngine;



namespace ScienceChecklist
{
   class HelpWindow : Window<ScienceChecklistAddon>
   {
      private GUIStyle labelStyle;
      private GUIStyle sectionStyle;
      private Vector2 scrollPosition;

      public HelpWindow(ScienceChecklistAddon Parent)
          : base("[x] Science! Help", 500, Screen.height * 0.75f / Parent.Config.UiScale, Parent)
      {
         scrollPosition = Vector2.zero;
      }

      protected override void ConfigureStyles()
      {
         base.ConfigureStyles();

         if (labelStyle == null)
         {
            labelStyle = new GUIStyle(_skin.label);
            labelStyle.wordWrap = true;
            labelStyle.fontStyle = FontStyle.Normal;
            labelStyle.fontSize = wScale(11);
            labelStyle.normal.textColor = Color.white;
            labelStyle.stretchWidth = true;
            labelStyle.stretchHeight = false;
            labelStyle.margin.bottom -= wScale(2);
            labelStyle.padding.bottom -= wScale(2);
         }
         if (sectionStyle == null)
         {
            sectionStyle = new GUIStyle(labelStyle);
            sectionStyle.fontStyle = FontStyle.Bold;
         }
      }



      protected override void DrawWindowContents(int windowID)
      {
         scrollPosition = GUILayout.BeginScrollView(scrollPosition);
         GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

         GUILayout.Label("[x] Science! by Z-Key Aerospace and Bodrick.", sectionStyle, GUILayout.ExpandWidth(true));
         GUILayout.Space(wScale(30));
         GUILayout.Label("About", sectionStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("[x] Science! creates a list of all possible science.  Use the list to find what is possible, to see what is left to accomplish, to decide where your Kerbals are going next.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("The four filter buttons at the bottom of the window are", sectionStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Show experiments available right now – based on you current ship and its situation", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Show experiments available on this vessel – based on your ship but including all known biomes", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Show all unlocked experiments – based on instruments you have unlocked and celestial bodies you have visited.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Show all experiments – shows everything.  You can hide this button", labelStyle, GUILayout.ExpandWidth(true));


         GUILayout.Space(wScale(20));
         GUILayout.Label("The text filter", sectionStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("To narrow your search, you may enter text into the filter eg \"kerbin’s shores\"", labelStyle, GUILayout.ExpandWidth(true));

         GUILayout.Label("Use – to mean NOT eg \"mun space -near\"", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("Use | to mean OR eg \"mun|minmus space\"", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Space(wScale(20));
         GUILayout.Label("The settings are", sectionStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Hide complete experiments – Any science with a full green bar is hidden.  It just makes it easier to see what is left to do.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Complete without recovery – Consider science in your spaceships as if it has been recovered.  You still need to recover to get the points.  It just makes it easier to see what is left to do.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Check debris – Science that survived a crash will be visible.  You may still be able to recover it.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Allow all filter – The \"All\" filter button shows science on planets you have never visited using instruments you have not invented yet.  Some people may consider it overpowered.  If you feel like a cheat, turn it off.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Adjust UI Size – Change the scaling of the UI.  Settings and Help window fonts will be re-sized on next restart.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Use blizzy78's toolbar – If you have blizzy78’s toolbar installed then place the [x] Science! button on that instead of the stock \"Launcher\" toolbar.", labelStyle, GUILayout.ExpandWidth(true));

         GUILayout.Space(wScale(20));
         GUILayout.Label("Did you know? (includes spoilers)", sectionStyle, GUILayout.ExpandWidth(true));


         GUILayout.Label("* In the VAB editor you can use the filter \"Show experiments available on this vessel\" to see what your vessel could collect before your launch it.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Does the filter \"mun space high\" show mun’s highlands?  – use \"mun space –near\" instead.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Need more science?  Go to Minmus.  It’s a little harder to get to but your fuel will last longer.  A single mission can collect thousands of science points before you have to come back.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* Generally moons are easier - it is more efficient to collect science from the surface of Ike or Gilly than from Duna or Eve.", labelStyle, GUILayout.ExpandWidth(true));
         GUILayout.Label("* All Kerbin’s biomes include both splashed and landed situations.  Landed at Kerbin’s water?  First build an aircraft carrier.", labelStyle, GUILayout.ExpandWidth(true));

         GUILayout.EndVertical();
         GUILayout.EndScrollView();

         GUILayout.Space(wScale(8));
      }
   }
}
