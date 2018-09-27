using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMaterialPreset {
	none,
	selected,
	movable,
	byId,

}

public class MaterialPreset : MonoBehaviour {
	[Header("General Setting")]
	public EMaterialPreset Preset = EMaterialPreset.none;
	public int PresetId = 0;
	public int pid;
	// public Renderer Renderer;
	public MaterialPropertyBlock PropertyBlock;
	// public Renderer Renderer;
	[Header("Configure")]
	public string[] TextureStrings;
	public Texture2D[] TextureValues;
	public string[] FloatStrings;
	public float[] FloatValues;
	public string[] ColorStrings;
	public Color[] ColorValues;
	public string[] VectorStrings;
	public Vector3[] VectorValues;
	static public int GlobalPresetId = 0;
	static public MaterialPreset Instance;
	static public MaterialPreset[] Data;

	static public MaterialPropertyBlock GetMaterialPreset(EMaterialPreset preset, int presetId = 0) {
		for(int i = 0; i < MaterialPreset.GlobalPresetId; i++) {
			MaterialPreset mp = MaterialPreset.Data[i];
			if (mp == null) continue;
			print("Preset:" + mp.Preset + "/"+ preset + ", PresetId:" + mp.PresetId +"/"+presetId);

			if(mp.Preset == preset && mp.PresetId == presetId) {
				return mp.PropertyBlock;
			}
		}
		print("GetMaterialPreset -> NULL");
		return null;
	}

	void Awake() {
		if (MaterialPreset.Instance == null) {
			MaterialPreset.Instance = this;
			MaterialPreset.Data = new MaterialPreset[1024];
			MaterialPreset.Instance = new GameObject("MaterialPresetMaster", typeof(MaterialPreset)).GetComponent<MaterialPreset>();
			DontDestroyOnLoad(Instance.gameObject);
			return;
		}
		if (name == "MaterialPresetMaster") { print(this + " is MaterialPresetMaster"); return; }	

		PropertyBlock = new MaterialPropertyBlock();
		Apply();

		MaterialPreset.Data[GlobalPresetId] = this;
		pid = GlobalPresetId++;
	}

	void Apply () {
		//if (PropertyBlock == null) return;

		int i = 0;
		foreach (string TextureString in TextureStrings) {
			PropertyBlock.SetTexture(TextureString, TextureValues[i]);
			i++;
		}

		// i = 0;
		// foreach (string FloatString in FloatStrings) {
		// 	PropertyBlock.SetFloat(FloatString, FloatValues[i]);
		// 	i++;
		// }

		// i = 0;
		// foreach (string ColorString in ColorStrings) {
		// 	PropertyBlock.SetColor(ColorString, ColorValues[i]);
		// 	i++;
		// }

		// i = 0;
		// foreach (string VectorString in VectorStrings) {
		// 	PropertyBlock.SetVector(VectorString, VectorValues[i]);
		// 	i++;
		// }
		//Renderer.SetPropertyBlock(PropertyBlock);
	}
}
