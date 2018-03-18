using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0,0.15]")]
	public partial class NetTileNetworkObject : NetworkObject
	{
		public const int IDENTITY = 7;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		private int _tile_Controlled_by;
		public event FieldEvent<int> tile_Controlled_byChanged;
		public Interpolated<int> tile_Controlled_byInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int tile_Controlled_by
		{
			get { return _tile_Controlled_by; }
			set
			{
				// Don't do anything if the value is the same
				if (_tile_Controlled_by == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_tile_Controlled_by = value;
				hasDirtyFields = true;
			}
		}

		public void Settile_Controlled_byDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_tile_Controlled_by(ulong timestep)
		{
			if (tile_Controlled_byChanged != null) tile_Controlled_byChanged(_tile_Controlled_by, timestep);
			if (fieldAltered != null) fieldAltered("tile_Controlled_by", _tile_Controlled_by, timestep);
		}
		private float _tile_Control_level;
		public event FieldEvent<float> tile_Control_levelChanged;
		public InterpolateFloat tile_Control_levelInterpolation = new InterpolateFloat() { LerpT = 0.15f, Enabled = true };
		public float tile_Control_level
		{
			get { return _tile_Control_level; }
			set
			{
				// Don't do anything if the value is the same
				if (_tile_Control_level == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_tile_Control_level = value;
				hasDirtyFields = true;
			}
		}

		public void Settile_Control_levelDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_tile_Control_level(ulong timestep)
		{
			if (tile_Control_levelChanged != null) tile_Control_levelChanged(_tile_Control_level, timestep);
			if (fieldAltered != null) fieldAltered("tile_Control_level", _tile_Control_level, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			tile_Controlled_byInterpolation.current = tile_Controlled_byInterpolation.target;
			tile_Control_levelInterpolation.current = tile_Control_levelInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _tile_Controlled_by);
			UnityObjectMapper.Instance.MapBytes(data, _tile_Control_level);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_tile_Controlled_by = UnityObjectMapper.Instance.Map<int>(payload);
			tile_Controlled_byInterpolation.current = _tile_Controlled_by;
			tile_Controlled_byInterpolation.target = _tile_Controlled_by;
			RunChange_tile_Controlled_by(timestep);
			_tile_Control_level = UnityObjectMapper.Instance.Map<float>(payload);
			tile_Control_levelInterpolation.current = _tile_Control_level;
			tile_Control_levelInterpolation.target = _tile_Control_level;
			RunChange_tile_Control_level(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _tile_Controlled_by);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _tile_Control_level);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (tile_Controlled_byInterpolation.Enabled)
				{
					tile_Controlled_byInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					tile_Controlled_byInterpolation.Timestep = timestep;
				}
				else
				{
					_tile_Controlled_by = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_tile_Controlled_by(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (tile_Control_levelInterpolation.Enabled)
				{
					tile_Control_levelInterpolation.target = UnityObjectMapper.Instance.Map<float>(data);
					tile_Control_levelInterpolation.Timestep = timestep;
				}
				else
				{
					_tile_Control_level = UnityObjectMapper.Instance.Map<float>(data);
					RunChange_tile_Control_level(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (tile_Controlled_byInterpolation.Enabled && !tile_Controlled_byInterpolation.current.UnityNear(tile_Controlled_byInterpolation.target, 0.0015f))
			{
				_tile_Controlled_by = (int)tile_Controlled_byInterpolation.Interpolate();
				//RunChange_tile_Controlled_by(tile_Controlled_byInterpolation.Timestep);
			}
			if (tile_Control_levelInterpolation.Enabled && !tile_Control_levelInterpolation.current.UnityNear(tile_Control_levelInterpolation.target, 0.0015f))
			{
				_tile_Control_level = (float)tile_Control_levelInterpolation.Interpolate();
				//RunChange_tile_Control_level(tile_Control_levelInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public NetTileNetworkObject() : base() { Initialize(); }
		public NetTileNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public NetTileNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
