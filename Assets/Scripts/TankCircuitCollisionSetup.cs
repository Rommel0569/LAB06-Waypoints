#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Ensures the tank scene has gameplay colliders on its four moving tanks.</summary>
[InitializeOnLoad]
public static class TankCircuitCollisionSetup
{
    private const string TankScenePath = "Assets/Scenes/TankRacing.unity";
    private static readonly string[] TankNames = { "BlueTank", "RedTank", "YellowTank", "GreenTank" };

    static TankCircuitCollisionSetup()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += EnsureLoadedTankScene;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.path == TankScenePath) EnsureTankCollisions(scene);
    }

    private static void EnsureLoadedTankScene()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == TankScenePath) EnsureTankCollisions(scene);
        }
    }

    private static void EnsureTankCollisions(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode) return;
        bool changed = false;
        foreach (string tankName in TankNames)
        {
            GameObject tank = FindByName(scene, tankName);
            if (tank == null) continue;

            BoxCollider collider = tank.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = Undo.AddComponent<BoxCollider>(tank);
                changed = true;
            }
            Bounds localBounds = GetLocalRendererBounds(tank.transform);
            Vector3 padding = new Vector3(0.12f, 0.08f, 0.12f);
            Vector3 size = localBounds.size + padding;
            size.x = Mathf.Max(size.x, 0.8f);
            size.y = Mathf.Max(size.y, 0.55f);
            size.z = Mathf.Max(size.z, 0.8f);
            if (collider.center != localBounds.center || collider.size != size)
            {
                collider.center = localBounds.center;
                collider.size = size;
                changed = true;
            }

            Rigidbody body = tank.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = Undo.AddComponent<Rigidbody>(tank);
                changed = true;
            }
            if (body.useGravity || body.isKinematic || body.constraints !=
                (RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ))
            {
                body.useGravity = false;
                body.isKinematic = false;
                body.constraints = RigidbodyConstraints.FreezePositionY |
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                body.mass = 3000f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                changed = true;
            }
        }

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }

    private static GameObject FindByName(Scene scene, string targetName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
                if (child.name == targetName) return child.gameObject;
        }
        return null;
    }

    private static Bounds GetLocalRendererBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            Bounds world = renderer.bounds;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 point = world.center + Vector3.Scale(world.extents, new Vector3(
                    (corner & 1) == 0 ? -1f : 1f,
                    (corner & 2) == 0 ? -1f : 1f,
                    (corner & 4) == 0 ? -1f : 1f));
                Vector3 local = root.InverseTransformPoint(point);
                if (!hasBounds)
                {
                    bounds = new Bounds(local, Vector3.zero);
                    hasBounds = true;
                }
                else bounds.Encapsulate(local);
            }
        }
        return hasBounds ? bounds : new Bounds(new Vector3(0f, 0.7f, 0f), new Vector3(3f, 1.4f, 5f));
    }
}
#endif
