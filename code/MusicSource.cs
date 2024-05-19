using Sandbox.Audio;
using System;

public sealed class MusicSource : Component
{
	[Property] public Action OnFinished { get; set; }
	[Property] public Action OnRepeated { get; set; }

	[Property] public string MusicPath { get; set; }
	[Property] public bool PlayOnStart { get; set; } = true;
	[Property]
	public float Volume
	{
		get => _volume;
		set
		{
			_volume = value;
			if ( _player != null )
			{
				_player.Volume = value;
			}
		}
	}
	private float _volume = 1f;
	[Property] public MixerHandle TargetMixer { get; set; }
	[Property] public bool Loop 
	{
		get => _loop;
		set
		{
			_loop = value;
			if ( _player is not null )
			{
				_player.Repeat = value;
			}
		}
	}
	private bool _loop = false;

	[Property, Range( 0, 1 )]
	public float Progress
	{
		get
		{
			if ( _player is null )
				return 0f;

			return _player.PlaybackTime / _player.Duration;
		}
		set
		{
			if ( _player is null )
				return;

			var newProgress = value.Clamp( 0, 1 );
			_player.Seek( _player.Duration * newProgress );
		}
	}


	private MusicPlayer _player;

	protected override void OnEnabled()
	{
		if ( PlayOnStart )
		{
			Play();
		}
	}

	protected override void OnUpdate()
	{
		if ( _player is null )
			return;

		_player.Position = Transform.Position;
	}

	protected override void OnDisabled()
	{
		Stop();
	}

	[Button( "Play" )]
	public void Play()
	{
		if ( !Game.IsPlaying )
			return;

		Stop();
		_player = CreatePlayer();
		ConfigurePlayer( _player );
	}

	private MusicPlayer CreatePlayer()
	{
		return MusicPlayer.Play( FileSystem.Mounted, MusicPath );
	}

	private void ConfigurePlayer( MusicPlayer player )
	{
		player.Volume = Volume;
		player.Position = Transform.Position;
		player.TargetMixer = TargetMixer.Get();
		player.Repeat = Loop;
		player.OnFinished += OnFinished;
		player.OnRepeated += OnRepeated;
	}

	[Button( "Stop" )]
	public void Stop()
	{
		if ( !Game.IsPlaying )
			return;

		_player?.Stop();
		_player?.Dispose();
		_player = null;
	}
}
