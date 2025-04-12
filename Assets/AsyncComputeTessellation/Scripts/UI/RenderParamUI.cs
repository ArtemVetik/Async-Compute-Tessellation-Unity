using System;
using System.Linq;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    internal class RenderParamUI : MonoBehaviour
    {
        [SerializeField] private DebugDropDown _renderMode;

        private DrawPipeline _drawPipeline;

        private void Update()
        {
            int renderModeInt = (int)_drawPipeline.RenderMode;
            if (_renderMode.UpdateValue(ref renderModeInt))
                _drawPipeline.RenderMode = (RenderMode)renderModeInt;
        }

        public void Initialize(DrawPipeline drawPipeline)
        {
            _drawPipeline = drawPipeline;

            var names = Enum.GetNames(typeof(RenderMode)).ToList();
            _renderMode.Setup("Render Mode", names);
        }
    }
}