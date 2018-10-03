﻿using UnityEngine;
using KSP.Localization;


namespace ScienceChecklist
{
	sealed class Noise : MonoBehaviour
	{
		private static readonly Logger _logger = new Logger( "Noise" );
		private static readonly string _file = "[x] Science!/bubbles";

		private void Awake( )
		{ }

		private void OnDestroy( )
		{ }

		public void PlaySound( )
		{
			if( gameObject == null )
			{
				_logger.Debug(Localizer.Format("#autoLOC_[x]_Science!_058")/*gameObject is null!*/ );
				return;
			}

			AudioSource audioSource = gameObject.GetComponent<AudioSource>( ) ?? gameObject.AddComponent<AudioSource>( );
			if( audioSource != null )
			{
				audioSource.spatialBlend = 0f;
				audioSource.panStereo = 0f;
				
				AudioClip Clip = null;
				Clip = GameDatabase.Instance.GetAudioClip( _file );
				if( Clip == null )
				{
					_logger.Debug(Localizer.Format("#autoLOC_[x]_Science!_059")/*No noise to play!*/ );
					return;
				}

				audioSource.PlayOneShot( Clip, Mathf.Clamp( GameSettings.UI_VOLUME, 0f, 1f ) );
			}
			else
				_logger.Debug(Localizer.Format("#autoLOC_[x]_Science!_060")/*No AudioSource*/ );
		}
	}
}
