using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Call_of_Cat_Lady
{
    public class Player
    {
        public Vector3 Position { get; set; }
        public float RotationY { get; set; }
        public Model Model { get; private set; }
        private Matrix[] boneTransforms;

        public Player(Model model, Vector3 startPosition)
        {
            Model = model;
            Position = startPosition;
            boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
        }

        public void Update(GameTime gameTime, Camera camera)
        {
            // The player's position and rotation will be driven by the camera
            Position = camera.Position - camera.GetForwardDirection() * 2; // Position slightly behind camera
            Position = new Vector3(Position.X, 1.0f, Position.Z); // Ensure player is on the ground
            
            // Make the player face the same direction as the camera
            RotationY = camera.Yaw + MathHelper.PiOver2;
        }

        public void Draw(Camera camera, Color ambientLight)
        {
            Matrix world = Matrix.CreateScale(0.01f) * // Scale down the model if it's too big
                           Matrix.CreateRotationY(RotationY) *
                           Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.LightingEnabled = true;
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = ambientLight.ToVector3();
                }
                mesh.Draw();
            }
        }
    }
}
