using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FountainPreview
{
    public class BasinVesselTag : Tag
    {
        public static void Hook()
        {
            Finder.GetLocationOverride += OverrideFountainLocation;
        }
        private static void OverrideFountainLocation(GetLocationEventArgs args)
        {
            if (args.LocationName != LocationNames.Vessel_Fragment_Basin)
            {
                return;
            }

            AbstractLocation loc = args.Current ?? Finder.GetLocationInternal(args.LocationName);
            loc.AddTag<BasinVesselTag>();
            args.Current = loc;
        }



        [JsonIgnore] private AbstractLocation parent;
        [JsonIgnore] private AbstractItem Target => parent.Placement.Items.FirstOrDefault(x => !x.WasEverObtained());
        [JsonIgnore] private GameObject previewGo;

        public override void Load(object parent)
        {
            this.parent = (AbstractLocation)parent;

            Events.AddSceneChangeEdit(SceneNames.Abyss_04, ReplaceObjectWithPreview);
        }

        public override void Unload(object parent)
        {
            Events.RemoveSceneChangeEdit(SceneNames.Abyss_04, ReplaceObjectWithPreview);
        }

        private void ReplaceObjectWithPreview(Scene scene)
        {
            if (Target is null) return;
            if (parent is ILocalHintLocation ilhl && !ilhl.GetItemHintActive()) return;
            if (parent.HasTag<ItemChanger.Tags.DisableItemPreviewTag>()) return;
            if (parent.Placement?.HasTag<ItemChanger.Tags.DisableItemPreviewTag>() ?? false) return;
            if (Target.HasTag<ItemChanger.Tags.DisableItemPreviewTag>()) return;

            Sprite sprite = Target.UIDef.GetSprite();

            GameObject orig = scene.FindGameObject("Wishing_Well_anims/Wishing_Well_normal");
            foreach (Renderer r in orig.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }

            previewGo = new();
            SpriteRenderer sr = previewGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            previewGo.transform.position = orig.transform.position;

            previewGo.SetActive(true);

            if (parent is ItemChanger.Locations.SpecialLocations.BasinFountainLocation)
            {
                scene.FindGameObject("Wishing_Well_anims")
                    .LocateMyFSM("Fountain Control")
                    .GetState("Appear")
                    .AddFirstAction(new Lambda(() =>
                    {
                        previewGo.SetActive(false);
                    }));
            }
        }
    }
}
