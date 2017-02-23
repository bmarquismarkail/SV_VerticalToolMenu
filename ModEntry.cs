using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using Microsoft.Xna.Framework.Input;

namespace SB_VerticalToolMenu
{

    public class ModEntry : Mod
    {
        VerticalToolBar verticalToolbar;
        bool isInitiated, modOverride;
        int currentToolIndex;
        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += initializeMod;
            GameEvents.UpdateTick += updatePosition;
            GameEvents.UpdateTick += hideTools;
            GameEvents.UpdateTick += checkCurrentTool;
            ControlEvents.KeyboardChanged += chooseToolKey;
            ControlEvents.MouseChanged += checkHoveredItem;

            isInitiated = false;
            modOverride = false;
        }

        private void checkCurrentTool(object sender, EventArgs e)
        {
            if (!isInitiated) return;
            if (Game1.player.CurrentToolIndex != currentToolIndex && modOverride)
            {
                Game1.player.CurrentToolIndex = currentToolIndex;
                modOverride = false;
            }
        }

        private void checkHoveredItem(object sender, EventArgsMouseStateChanged e)
        {
            if (Mouse.GetState().ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftTrigger) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.RightTrigger) )
            {
                int num = Mouse.GetState().ScrollWheelValue > Game1.oldMouseState.ScrollWheelValue ? -1 : 1;
                if (Game1.options.gamepadControls && num == 0)
                    num = GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftTrigger) ? -1 : 1;
                if (Game1.options.invertScrollDirection)
                    num *= -1;

                currentToolIndex = Game1.player.CurrentToolIndex += num;
                if (num < 0)
                {
                    if (currentToolIndex < 0)
                    {
                        currentToolIndex = Convert.ToInt32(verticalToolbar.buttons[verticalToolbar.numToolsinToolbar - 1].name);
                    }
                    else if (currentToolIndex > 11 && currentToolIndex < Convert.ToInt32(verticalToolbar.buttons[0].name))
                    {
                        currentToolIndex = 11;
                    }
                }
                else if (num > 0)
                {
                    if (currentToolIndex > Convert.ToInt32(verticalToolbar.buttons[verticalToolbar.numToolsinToolbar - 1].name))
                    {
                        currentToolIndex = 0;
                    }
                    else if (currentToolIndex > 11 && currentToolIndex < Convert.ToInt32(verticalToolbar.buttons[0].name))
                    {
                        currentToolIndex = Convert.ToInt32(verticalToolbar.buttons[0].name);
                    }
                }
                modOverride = true;
            }
        }

        private void chooseToolKey(object sender, EventArgsKeyboardStateChanged e)
        {
            if (!Game1.player.UsingTool && e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                if (e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad1))
                    currentToolIndex = Convert.ToInt32(verticalToolbar.buttons[0].name);
                else if (e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad2))
                    currentToolIndex = Convert.ToInt32(verticalToolbar.buttons[1].name);
                else if (e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad3))
                    currentToolIndex = Convert.ToInt32(verticalToolbar.buttons[2].name);
                else if (e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad4))
                    currentToolIndex = Convert.ToInt32(verticalToolbar.buttons[3].name);
                else if (e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) && e.NewState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad5))
                    currentToolIndex = Convert.ToInt32(verticalToolbar.buttons[4].name);

                modOverride = true;
            }
        }

        private void hideTools(object sender, EventArgs e)
        {
            if (!isInitiated) return;

            for(int count = Game1.player.items.Count; count < 36; ++count)
            {
                    Game1.player.items.Add((Item)null);
            }
            for (int i = 0; i < Game1.player.maxItems;)
            {
                if(Game1.player.items[i] == null)
                {
                    i++;
                    continue;
                }
                if (Game1.player.items[i] is Axe || Game1.player.items[i] is Hoe
                    || Game1.player.items[i] is Pickaxe || (Game1.player.items[i] is MeleeWeapon && (Game1.player.items[i] as MeleeWeapon).Name.Equals("Scythe"))
                    || Game1.player.items[i] is FishingRod)
                {
                    Game1.player.items.Add(Game1.player.items[i]);
                    Game1.player.items.Remove(Game1.player.items[i]);
                    if(Game1.player.items[35] != null)
                        Game1.player.items.Insert( 35, (Item)null);
                    continue;
                }
                i++;
            }
        }

        private void updatePosition(object sender, EventArgs e)
        {
            if (isInitiated)
            {
                verticalToolbar.xPositionOnScreen = getToolbar().xPositionOnScreen - ( verticalToolbar.width / 2 );
            }
        }

        private Toolbar getToolbar()
        {
            for (int index = 0; index < Game1.onScreenMenus.Count; ++index)
            {
                if (Game1.onScreenMenus[index] is Toolbar)
                {
                    return Game1.onScreenMenus[index] as Toolbar;
                }
            }

            return null;
        }

        private void initializeMod(object sender, EventArgs e)
        {
            verticalToolbar = new VerticalToolBar(getToolbar().xPositionOnScreen - (VerticalToolBar.getInitialWidth() / 2), Game1.viewport.Height - VerticalToolBar.getInitialHeight());
            Game1.onScreenMenus.Add(verticalToolbar);

            currentToolIndex = Game1.player.CurrentToolIndex;
            isInitiated = true;
        }
    }
}