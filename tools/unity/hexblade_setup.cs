// Настройка Hexblade в открытом Unity Editor через `unity command eval_file` (без domain reload).
// Prefab и проверочный кадр собираются в preview-сцене: открытая сцена пользователя не меняется.
// eval оборачивает код в тело метода, поэтому using недоступны и типы указаны полностью.

const string Dir = "Assets/Heroes/Hexblade";
AssetDatabase.ImportAsset(Dir, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);

foreach (var name in new[] { "Hexblade_Albedo", "Hexblade_Emission" })
{
    var ti = (TextureImporter)AssetImporter.GetAtPath($"{Dir}/{name}.png");
    ti.sRGBTexture = true;
    ti.maxTextureSize = 2048;
    ti.mipmapEnabled = true;
    ti.SaveAndReimport();
}

var mi = (ModelImporter)AssetImporter.GetAtPath($"{Dir}/Hexblade.fbx");
mi.materialImportMode = ModelImporterMaterialImportMode.None;
mi.animationType = ModelImporterAnimationType.None;
mi.importAnimation = false;
mi.importCameras = false;
mi.importLights = false;
mi.SaveAndReimport();

var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/Hexblade_Albedo.png");
var emission = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/Hexblade_Emission.png");
var matPath = $"{Dir}/Hexblade.mat";
var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
if (mat == null)
{
    mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    AssetDatabase.CreateAsset(mat, matPath);
}
mat.SetTexture("_BaseMap", albedo);
mat.SetColor("_BaseColor", Color.white);
mat.SetFloat("_Metallic", 0f);
mat.SetFloat("_Smoothness", .3f);
mat.SetTexture("_EmissionMap", emission);
mat.SetColor("_EmissionColor", Color.white * 2.5f);
mat.EnableKeyword("_EMISSION");
mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
EditorUtility.SetDirty(mat);

var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{Dir}/Hexblade.fbx");
var preview = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var hero = (GameObject)PrefabUtility.InstantiatePrefab(model, preview);
hero.name = "Hexblade";
var renderer = hero.GetComponentInChildren<MeshRenderer>();
renderer.sharedMaterial = mat;
var prefab = PrefabUtility.SaveAsPrefabAsset(hero, $"{Dir}/Hexblade.prefab");
AssetDatabase.SaveAssets();

var mesh = hero.GetComponentInChildren<MeshFilter>().sharedMesh;
var b = renderer.bounds;
// Маска (лицо) самая выступающая вперед часть головы: среднее Z вершин выше 2.3 м показывает, куда смотрит герой.
float faceZ = 0f; int faceN = 0;
var tr = renderer.transform;
foreach (var v in mesh.vertices)
{
    var w = tr.TransformPoint(v);
    if (w.y > 2.0f && w.y < 2.25f && Mathf.Abs(w.x) < .06f) { faceZ += w.z; faceN++; }
}


// PreviewRenderUtility рендерит через активный SRP (URP) в изолированной сцене превью.
var pru = new PreviewRenderUtility();
pru.camera.fieldOfView = 30f;
pru.camera.nearClipPlane = .1f;
pru.camera.farClipPlane = 50f;
pru.camera.clearFlags = CameraClearFlags.SolidColor;
pru.camera.backgroundColor = new Color(.04f, .05f, .08f);
pru.camera.transform.position = new Vector3(2.6f, 2.4f, 6.4f);
pru.camera.transform.LookAt(new Vector3(0, 1.35f, 0));
pru.lights[0].intensity = 1.4f;
pru.lights[0].transform.rotation = Quaternion.Euler(35f, 200f, 0f);
pru.lights[1].intensity = .8f;
pru.lights[1].transform.rotation = Quaternion.Euler(20f, 20f, 0f);
pru.ambientColor = new Color(.25f, .27f, .35f);
pru.BeginStaticPreview(new Rect(0, 0, 900, 1100));
pru.DrawMesh(mesh, tr.localToWorldMatrix, mat, 0);
pru.camera.Render();
var shot = pru.EndStaticPreview();
var shotPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../verification/hexblade-unity.png"));
System.IO.File.WriteAllBytes(shotPath, shot.EncodeToPNG());
pru.Cleanup();

return $"prefab={AssetDatabase.GetAssetPath(prefab)} tris={mesh.triangles.Length / 3} submeshes={mesh.subMeshCount} " +
       $"verts={mesh.vertexCount} boundsMin={b.min} boundsMax={b.max} faceZ={(faceN > 0 ? faceZ / faceN : float.NaN):F3} " +
       $"faceSamples={faceN} shader={mat.shader.name} shot={shotPath}";
