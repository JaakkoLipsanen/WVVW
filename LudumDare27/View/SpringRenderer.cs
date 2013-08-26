using System.Collections.ObjectModel;
using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using WVVW.Model;

namespace WVVW.View
{
    public class SpringRenderer : FlaiRenderer
    {
        private readonly ReadOnlyCollection<Spring> _springs;
        public SpringRenderer(ReadOnlyCollection<Spring> springs)
        {
            _springs = springs;
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            foreach (Spring spring in _springs)
            {
                graphicsContext.PrimitiveRenderer.DrawLine(graphicsContext, spring.StartPoint, spring.EndPoint, (spring.IsTriggered ? Color.DarkGray : Color.White) * 0.625f, Spring.Thickness);
            }
        }
    }
}
