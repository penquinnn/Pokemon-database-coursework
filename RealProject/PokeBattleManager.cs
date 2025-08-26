using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RealProject
{
    static class PokeBattleManager
    {
        static Dictionary<int, Vector2> battleBackgroundsDic;
        static Texture2D battleBackgroundsSprite;
        static int currentArea = 0;
        public static PokeBattleInstance currentBattle;

        static Action backAction;

        enum BattleStates
        {
            BattleStart,
            PlayerTurn,
            DoingTurn,
            Win,
            Loss,
            Ran
        }

        static BattleStates state;

        public static void Initialiize(ContentManager content)
        {
            battleBackgroundsDic = new Dictionary<int, Vector2>();

            battleBackgroundsDic.Add(0, new Vector2(6, 6));
            battleBackgroundsDic.Add(1, new Vector2(329, 6));
            battleBackgroundsDic.Add(2, new Vector2(652, 6));

            battleBackgroundsDic.Add(3, new Vector2(6, 141));
            battleBackgroundsDic.Add(4, new Vector2(329, 141));
            battleBackgroundsDic.Add(5, new Vector2(652, 141));

            battleBackgroundsDic.Add(6, new Vector2(6, 276));
            battleBackgroundsDic.Add(7, new Vector2(329, 276));
            battleBackgroundsDic.Add(8, new Vector2(652, 276));

            battleBackgroundsDic.Add(9, new Vector2(6, 411));

            battleBackgroundsSprite = content.Load<Texture2D>("battleBackgrounds");
        }

        public static void Update()
        {
            if (InputManager.GetKeyDown(Keys.Escape) || InputManager.GetMouseButtonDown(4))
            {
                if (state == BattleStates.PlayerTurn)
                {
                    backAction?.Invoke();
                    //ShowOptions();
                }
            }
        }

        public static void BeginBattle(PokemonInstance[] party, PokemonInstance[] enemyTeam, int enemyAi, int background)
        {
            currentBattle = new PokeBattleInstance(party, enemyTeam, true, enemyAi);
            currentArea = background;
            state = BattleStates.BattleStart;

            GameStateManager.ChangeState(GameStateManager.GameState.Battle);
            CoroutineManager.Start(IntroSequence());
        }

        static IEnumerator<object> IntroSequence()
        {
            yield return CoroutineManager.Start(UIManager.DoTransitionAnimation(null));

            Vector2 pos = battleBackgroundsDic[currentArea];
            UIManager.battleRects["Image_Background"].SetSprite(Global.CropTexture(battleBackgroundsSprite, new Rectangle((int)pos.X, (int)pos.Y, 320, 180)));
            UIManager.battleRects["Image_Background"].SetEnabled(true);
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
            UIManager.battleRects["Image_AttackMenu"].SetEnabled(false);
            UIManager.battleRects["Image_PlayerPokemon"].SetEnabled(false);
            UIManager.battleRects["Image_EnemyPokemon"].SetEnabled(false);

            if(currentBattle.isWildBattle)
                yield return Global.StartTypewriter($"A  Wild  {currentBattle.enemyTeam[0].name.ToUpper()}  appears!", currentBattle.battleTextPos);
            else
                yield return Global.StartTypewriter("TRAINER  {Something}  would  like  to  battle!", currentBattle.battleTextPos);

            yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

            if (!currentBattle.isWildBattle)
            {
                yield return Global.StartTypewriter($"TRAINER  (SOMETHING)  sent  out  {currentBattle.enemyTeam[0].name}!", currentBattle.battleTextPos);
                yield return Global.battleSpeed;
            }

            Enemy_SwitchInPokemon(0);
            UIManager.battleRects["Image_EnemyPokemon"].SetEnabled(true);

            yield return Global.StartTypewriter($"Go!  {currentBattle.playerTeam[0].name.ToUpper()}!", currentBattle.battleTextPos);
            yield return Global.battleSpeed;

            Player_SwitchInPokemon(0);
            UIManager.battleRects["Image_PlayerPokemon"].SetEnabled(true);

            PlayerTurn();
        }

        //static void DisplayUI()
        //{
        //    Vector2 pos = battleBackgroundsDic[currentArea];
        //    Global.StartTypewriter($"What  will  {DatabaseManager.GetBasePokemonData(1, 1).name}  do?", new Vector2(Global.screenWidth * 0.035f, Global.screenHeight * 0.78f));

        //    Enemy_SwitchInPokemon(0);
        //    Player_SwitchInPokemon(0);
        //}

        static void PlayerTurn()
        {
            Global.currentTypeText = $"What  will  {currentBattle.playerTeam[0].name.ToUpper()}  do?";
            state = BattleStates.PlayerTurn;
            UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(false);
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(true);
        }

        public static void ChangeHPText(string hpName, string sliderName)
        {
            UITextBox healthText = (UITextBox)UIManager.battleRects[hpName]; 
            UISlider slider = (UISlider)UIManager.battleRects[sliderName];
            healthText.SetText($"{Math.Ceiling(slider.value)}/{slider.maxValue}");
        }
        public static void ShowOptions()
        {
            //UIManager.battleRects["Image_BagMenu"].SetEnabled(false);
            //UIManager.battleRects["Image_PokemonMenu"].SetEnabled(false);
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(true);
            UIManager.battleRects["Image_AttackMenu"].SetEnabled(false);
            Global.currentTypeText = $"What  will  {currentBattle.playerTeam[0].name.ToUpper()}  do?";
        }

        public static void OnRunChosen()
        {
            if(!currentBattle.isWildBattle)
                CoroutineManager.Start(RunSequence(false));
            else if (IsPlayerFaster())
                CoroutineManager.Start(RunSequence(true));
            else
            {
                currentBattle.runAttempts++;

                float odds = ((currentBattle.playerTeam[currentBattle.playerTeamIndex].speStat * 32f) / (currentBattle.enemyTeam[currentBattle.enemyTeamIndex].speStat / 4f)) + 30 * currentBattle.runAttempts;

                int chance = Global.random.Next(0, 256);

                if(chance < odds)
                    CoroutineManager.Start(RunSequence(true));
                else
                {
                    CoroutineManager.Start(RunSequence(false));
                }
            }
        }

        static IEnumerator<object> RunSequence(bool success)
        {
            state = BattleStates.Ran;
            
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
            UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);

            if (!currentBattle.isWildBattle)
            {
                UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
                UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);

                yield return Global.StartTypewriter("Can't  escape!", currentBattle.battleTextPos);

                yield return Global.battleSpeed;

                Global.currentTypeText = "";
                ShowOptions();
                yield break;
            }

            if (success)
            {
                yield return Global.StartTypewriter("Got  away  safely!", currentBattle.battleTextPos);
                yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

                GameStateManager.ChangeState(GameStateManager.GameState.Overworld);

                CoroutineManager.Start(UIManager.DoTransitionAnimation(() =>
                {
                    UIManager.battleRects["Image_Background"].SetEnabled(false);
                    Global.currentTypeText = "";
                }));
            }
            else
            {
                yield return Global.StartTypewriter("Can't  escape!", currentBattle.battleTextPos);
                yield return Global.battleSpeed;

                CoroutineManager.Start(OnAttackChosen(-1));
            }
        }

        static bool IsPlayerFaster()
        {
            float enemyMultiplier = currentBattle.enemyTeam[currentBattle.enemyTeamIndex].statusCondition.primaryCondition == PrimaryStatusConditions.Paralysis ? 0.5f : 1;
            float playerMultiplier = currentBattle.playerTeam[currentBattle.playerTeamIndex].statusCondition.primaryCondition == PrimaryStatusConditions.Paralysis ? 0.5f : 1;

            return currentBattle.enemyTeam[currentBattle.enemyTeamIndex].speStat*enemyMultiplier <= currentBattle.playerTeam[currentBattle.playerTeamIndex].speStat*playerMultiplier;
        }

        public static void ShowAttacks()
        {
            backAction = () => ShowOptions();

            UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
            UIManager.battleRects["Image_AttackMenu"].SetEnabled(true);
            Global.currentTypeText = "";
        }

        static MoveInstance EnemyChooseMove()
        {
            MoveInstance moveChosen = null;

            if (currentBattle.aiLevel == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int rand = Global.random.Next(0, currentBattle.enemyTeam[currentBattle.enemyTeamIndex].moveset.Length);

                    moveChosen = currentBattle.enemyTeam[currentBattle.enemyTeamIndex].moveset[rand];

                    if (moveChosen.currentPP > 0)
                        i = 100;
                }
            }

            if (moveChosen == null)
            {
                Debug.WriteLine("Couldn't find a suitable move.");
                moveChosen = DatabaseManager.GetBaseMoveInstance(2);
            }

            return moveChosen;
        }

        public static int CalculateAttack(PokemonInstance attackingPoke, PokemonInstance defendingPoke, MoveInstance moveChosen)
        {
            int damageOutput = 0;
            float multiplier = 1;
            //Account for accuracy

            if (moveChosen.property == 0) // Status
            {
                //implement
            }
            else if (moveChosen.property == 1) // Physical
            {
                multiplier = 1;
                float burn = attackingPoke.statusCondition.primaryCondition == PrimaryStatusConditions.Burn ? 0.5f : 1;

                multiplier *= DatabaseManager.GetTypeEffectiveness(moveChosen.typeID, defendingPoke.type1, defendingPoke.type2);

                if (moveChosen.typeID == attackingPoke.type1 || moveChosen.typeID == attackingPoke.type2)
                    multiplier *= 1.5f;

                //Account for crits
                //Account for random between 1 and 0.85

                damageOutput = (int)Math.Round(((2 * (float)attackingPoke.level / 5f + 2) * moveChosen.damage * attackingPoke.atkStat / defendingPoke.defStat / 50 + 2) * multiplier * burn, 0);

                Debug.WriteLine($"Should be doing: {damageOutput} physical damage to {defendingPoke.name}.");
            }
            else if (moveChosen.property == 2) // Special
            {
                multiplier = 1;
                
                multiplier *= DatabaseManager.GetTypeEffectiveness(moveChosen.typeID, defendingPoke.type1, defendingPoke.type2);

                if (moveChosen.typeID == attackingPoke.type1 || moveChosen.typeID == attackingPoke.type2)
                    multiplier *= 1.5f;

                //Account for crits
                //Account for random between 1 and 0.85

                damageOutput = (int)Math.Round(((2 * (float)attackingPoke.level / 5f + 2) * moveChosen.damage * attackingPoke.spaStat / defendingPoke.spdStat / 50 + 2) * multiplier, 0);

                Debug.WriteLine($"Should be doing: {damageOutput} special damage to {defendingPoke.name}.");
            }
            else //Idk some type of fallback or something
                return 0;

            return damageOutput;
        }

        public static bool AccuracyCheck(int chanceToSucceed)
        {
            return (Global.random.Next(1, 101) <= chanceToSucceed);
            //if (Global.random.Next(1, 101) <= chance)
            //{
            //    if (hitString != null)
            //    {
            //        yield return Global.StartTypewriter(hitString, currentBattle.battleTextPos);
            //        yield return Global.battleSpeed;
            //    }

            //    if (hitAction != null)
            //        hitAction();
            //}
            //else
            //{
            //    if (missString != null)
            //    {
            //        yield return Global.StartTypewriter(missString, currentBattle.battleTextPos);
            //        yield return Global.battleSpeed;
            //    }

            //    if(isPlayer)
            //        currentBattle.playerCanAct = false;
            //    else
            //        currentBattle.enemyCanAct = false;
            //}
        }

        public static IEnumerator<object> OnAttackChosen(int attackIndex)
        {
            PokemonInstance currentPoke = currentBattle.playerTeam[currentBattle.playerTeamIndex];
            PokemonInstance enemyPoke = currentBattle.enemyTeam[currentBattle.enemyTeamIndex];
            MoveInstance enemyMove = EnemyChooseMove();
            float multiplier = 1;
            int damage;
            backAction = null;

            currentBattle.playerCanAct = true;
            currentBattle.enemyCanAct = true;

            if (attackIndex == -1)
            {
                currentBattle.playerCanAct = false;
            }

            MoveInstance moveChosen = null;

            if (currentBattle.playerCanAct)
                moveChosen = currentPoke.moveset[attackIndex];

            if (IsPlayerFaster()) //Player faster or equal
            {
                yield return CoroutineManager.Start(CheckProhibitingStatuses(currentPoke, enemyPoke, currentPoke.name.ToUpper(), true));

                if (currentBattle.playerCanAct == true)
                {
                    multiplier *= DatabaseManager.GetTypeEffectiveness(moveChosen.typeID, enemyPoke.type1, enemyPoke.type2);
                    damage = CalculateAttack(currentPoke, enemyPoke, moveChosen);

                    moveChosen.currentPP--;

                    yield return CoroutineManager.Start(AttackSequence(currentPoke, enemyPoke, moveChosen.name, damage, multiplier, true));
                    yield return currentBattle.CheckDeath(enemyPoke, false);
                }

                yield return CoroutineManager.Start(CheckProhibitingStatuses(enemyPoke, currentPoke, $"{currentBattle.enemyPrefix}  {enemyPoke.name.ToUpper()}", false));

                if (currentBattle.enemyCanAct == true)
                {
                    damage = CalculateAttack(enemyPoke, currentPoke, enemyMove);

                    enemyMove.currentPP--;

                    yield return CoroutineManager.Start(AttackSequence(enemyPoke, currentPoke, enemyMove.name, damage, multiplier, false));
                    yield return currentBattle.CheckDeath(currentPoke, true);
                }
            }
            else //Enemy faster
            {
                yield return CoroutineManager.Start(CheckProhibitingStatuses(enemyPoke, currentPoke, $"{currentBattle.enemyPrefix}  {enemyPoke.name.ToUpper()}", false));

                if (currentBattle.enemyCanAct == true)
                {
                    multiplier *= DatabaseManager.GetTypeEffectiveness(enemyMove.typeID, currentPoke.type1, currentPoke.type2);
                    damage = CalculateAttack(enemyPoke, currentPoke, enemyMove);

                    enemyMove.currentPP--;

                    yield return CoroutineManager.Start(AttackSequence(enemyPoke, currentPoke, enemyMove.name, damage, multiplier, false));
                    yield return currentBattle.CheckDeath(currentPoke, true);
                }

                yield return CoroutineManager.Start(CheckProhibitingStatuses(currentPoke, enemyPoke, currentPoke.name.ToUpper(), true));

                if (currentBattle.playerCanAct == true)
                {
                    multiplier = 1;
                    multiplier *= DatabaseManager.GetTypeEffectiveness(moveChosen.typeID, enemyPoke.type1, enemyPoke.type2);
                    damage = CalculateAttack(currentPoke, enemyPoke, moveChosen);

                    moveChosen.currentPP--;

                    yield return CoroutineManager.Start(AttackSequence(currentPoke, enemyPoke, moveChosen.name, damage, multiplier, true));
                    yield return currentBattle.CheckDeath(enemyPoke, false);
                }
            }

            //Do status stuff
            CheckDamagingStatuses(enemyPoke, $"{currentBattle.enemyPrefix}  {enemyPoke.name.ToUpper()}", false);
            yield return currentBattle.CheckDeath(enemyPoke, false);

            CheckDamagingStatuses(currentPoke, currentPoke.name.ToUpper(), true);
            yield return currentBattle.CheckDeath(currentPoke, true);

            PlayerTurn();
        }

        public static IEnumerator<object> CheckProhibitingStatuses(PokemonInstance poke, PokemonInstance defendingPoke, string displayName, bool isPlayer)
        {
            if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Freeze) // Frozen
            {
                yield return Global.StartTypewriter($"{displayName}  is  frozen...", currentBattle.battleTextPos);
                yield return Global.battleSpeed;

                if(AccuracyCheck(20))
                {
                    yield return Global.StartTypewriter($"{displayName}  broke  the  ice!", currentBattle.battleTextPos);
                    poke.statusCondition = null;
                }
                else
                {
                    yield return Global.StartTypewriter($"{displayName}  is  frozen  solid!", currentBattle.battleTextPos);

                    if (isPlayer) currentBattle.playerCanAct = false;
                    else currentBattle.enemyCanAct = false;
                }
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Paralysis) // Paralyzed
            {
                yield return Global.StartTypewriter($"{displayName}  is  paralyzed...", currentBattle.battleTextPos);
                yield return Global.battleSpeed;

                if (!AccuracyCheck(75))
                {
                    yield return Global.StartTypewriter($"{displayName}  cannot  move  due  to  its  paralysis!", currentBattle.battleTextPos);

                    if (isPlayer) currentBattle.playerCanAct = false;
                    else currentBattle.enemyCanAct = false;

                    poke.statusCondition = null;
                }
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Sleep) // Sleep
            {
                if (poke.statusCondition.turnCount > 0)
                {
                    yield return Global.StartTypewriter($"{displayName}  is  asleep...", currentBattle.battleTextPos);

                    if (isPlayer) currentBattle.playerCanAct = false;
                    else currentBattle.enemyCanAct = false;

                    poke.statusCondition.turnCount--;
                }
                else
                {
                    yield return Global.StartTypewriter($"{displayName}  woke  up!", currentBattle.battleTextPos);
                    poke.statusCondition = new StatusEffects(PrimaryStatusConditions.None);
                }

                yield return Global.battleSpeed;
            } else // Check volatile prohibiting statuses
            {
                if (GetVolatileStatus(poke, VolatileStatusConditions.Confusion) != null) // Confusion
                {
                    StatusEffects confuse = GetVolatileStatus(poke, VolatileStatusConditions.Confusion);
                    if (confuse.turnCount > 0)
                    {
                        yield return Global.StartTypewriter($"{displayName}  is  confused...", currentBattle.battleTextPos);
                        yield return Global.battleSpeed;

                        if (!AccuracyCheck(67))
                        {
                            yield return Global.StartTypewriter($"It hurt itself in its confusion!", currentBattle.battleTextPos);
                            yield return Global.battleSpeed;

                            MoveInstance confuseMove = new MoveInstance(0, "", 40, 100, 19, 100, 1);
                            yield return currentBattle.TakeDamage(!isPlayer, CalculateAttack(poke, defendingPoke, confuseMove));
                            yield return currentBattle.CheckDeath(poke, isPlayer);

                            if (isPlayer) currentBattle.playerCanAct = false;
                            else currentBattle.enemyCanAct = false;
                        }

                        confuse.turnCount--;
                    }
                    else
                    {
                        yield return Global.StartTypewriter($"{displayName}  snapped  out  of  its  confusion!", currentBattle.battleTextPos);
                        yield return Global.battleSpeed;
                        poke.volatileStatuses.Remove(confuse);
                    }
                }
            }
        }

        public static IEnumerator<object> CheckDamagingStatuses(PokemonInstance poke, string displayName, bool isPlayer)
        {
            if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Burn)
            {
                yield return Global.StartTypewriter($"{displayName}  is  hurt  by  its  poison!", currentBattle.battleTextPos);
                yield return currentBattle.TakeDamage(!isPlayer, MathHelper.Clamp(poke.hpStat / 8, 1, 1000));
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Poison)
            {
                yield return Global.StartTypewriter($"{displayName}  is  hurt  by  its  burn!", currentBattle.battleTextPos);
                yield return currentBattle.TakeDamage(!isPlayer, MathHelper.Clamp(poke.hpStat / 16, 1, 1000));
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Toxic)
            {
                poke.statusCondition.turnCount++;

                yield return Global.StartTypewriter($"{displayName}  is  hurt  by  its  poison!", currentBattle.battleTextPos);
                yield return currentBattle.TakeDamage(!isPlayer, MathHelper.Clamp(poke.statusCondition.turnCount * poke.hpStat / 16, 1, 1000));
            }
        }

        public static StatusEffects GetVolatileStatus(PokemonInstance poke, VolatileStatusConditions condition)
        {
            foreach(var v in poke.volatileStatuses)
            {
                if (v.volatileCondition == condition)
                    return v;
            }

            return null;
        }

        static IEnumerator<object> AttackSequence(PokemonInstance attackingPoke, PokemonInstance defendingPoke, string attackName, int damage, float effective, bool playerAttack)
        {
            UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);
            UIManager.battleRects["Image_AttackMenu"].SetEnabled(false);

            state = BattleStates.DoingTurn;

            if(playerAttack)
                yield return Global.StartTypewriter($"{attackingPoke.name.ToUpper()}  used  {attackName.ToUpper()}!", currentBattle.battleTextPos);
            else
                yield return Global.StartTypewriter($"{currentBattle.enemyPrefix}  {attackingPoke.name.ToUpper()}  used  {attackName.ToUpper()}!", currentBattle.battleTextPos);

            yield return Global.battleSpeed;

            //Attack animations and stuff

            yield return currentBattle.TakeDamage(playerAttack, damage);

            if (damage > 0)
            {
                if (effective == 0)
                    yield return Global.StartTypewriter($"It  doesn't  affect  {defendingPoke.name.ToUpper()}!", currentBattle.battleTextPos);
                else if (effective < 1)
                    yield return Global.StartTypewriter($"It's  not  very  effective...", currentBattle.battleTextPos);
                else if (effective > 1)
                    yield return Global.StartTypewriter($"It's  super  effective!", currentBattle.battleTextPos);
            }

            if (effective != 1) yield return Global.battleSpeed;

            state = BattleStates.PlayerTurn;
        }

        public static IEnumerator<object> FaintSequence(PokemonInstance faintedPoke, bool isPlayer)
        {
            int option = 0;
            if (isPlayer)
                option = 1;
            else if (currentBattle.isWildBattle)
                option = 2;
            else
                option = 3;

            yield return Global.battleSpeed;

            switch (option)
            {
                case 1: //Player pokemon faints
                    yield return Global.StartTypewriter($"{faintedPoke.name.ToUpper()}  fainted!", currentBattle.battleTextPos);

                    break;
                case 2: // Wild pokemon faints
                    yield return Global.StartTypewriter($"{currentBattle.enemyPrefix}  {faintedPoke.name.ToUpper()}  fainted!", currentBattle.battleTextPos);

                    //Do animation and cry
                    yield return Global.battleSpeed;

                    PokemonInstance playerPoke = currentBattle.playerTeam[currentBattle.playerTeamIndex];
                    int totalExpGain = faintedPoke.CalculateExpGainFromKO(playerPoke.level);

                    yield return Global.StartTypewriter($"{playerPoke.name.ToUpper()}  gained  {totalExpGain}  EXP.  Points!", currentBattle.battleTextPos);
                    yield return Global.battleSpeed;

                    int expGain = Math.Clamp(totalExpGain, 0, playerPoke.maxExp - playerPoke.currentExp);

                    Debug.WriteLine(totalExpGain);

                    currentBattle.isAnimating = true;
                    yield return CoroutineManager.Start(currentBattle.AnimateSlider((UISlider)UIManager.battleRects["Slider_PlayerExp"], playerPoke.currentExp + expGain));

                    totalExpGain -= expGain;
                    playerPoke.GainExp(expGain);

                    while (totalExpGain > 0)
                    {
                        //Play sound
                        Debug.WriteLine(playerPoke.level);

                        yield return Global.battleSpeed;

                        ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).SetValue(0);

                        ((UITextBox)UIManager.battleRects["Text_PlayerLevel"]).SetText($"Lv{playerPoke.level}");
                        yield return Global.StartTypewriter($"{playerPoke.name.ToUpper()}  grew  to  Lv.  {playerPoke.level}!", currentBattle.battleTextPos);

                        yield return Global.battleSpeed;

                        ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).maxValue = playerPoke.maxExp;

                        expGain = MathHelper.Clamp(totalExpGain, 0, playerPoke.maxExp - playerPoke.currentExp);

                        currentBattle.isAnimating = true;
                        yield return CoroutineManager.Start(currentBattle.AnimateSlider((UISlider)UIManager.battleRects["Slider_PlayerExp"], playerPoke.currentExp + expGain));

                        totalExpGain -= expGain;
                        playerPoke.GainExp(expGain);
                    }

                    yield return Global.battleSpeed;

                    yield return Global.StartTypewriter($"Player   defeated  wild  {faintedPoke.name.ToUpper()}!", currentBattle.battleTextPos);

                    yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

                    //yield return Global.StartTypewriter($"(PLAYER NAME)   got  $(MONEY)  for  winning!", currentBattle.battleTextPos);

                    //yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

                    GameStateManager.ChangeState(GameStateManager.GameState.Overworld);

                    CoroutineManager.Start(UIManager.DoTransitionAnimation(() =>
                    {
                        UIManager.battleRects["Image_Background"].SetEnabled(false);
                        Global.currentTypeText = "";
                    }));



                    break;
                case 3: // Trainer pokemon faints
                    yield return Global.StartTypewriter($"{currentBattle.enemyPrefix}  {faintedPoke.name.ToUpper()}  fainted!", currentBattle.battleTextPos);

                    break;
            }
        }

        public static void Player_SwitchInPokemon(int index)
        {
            currentBattle.playerTeamIndex = index;
            currentBattle.playerTeam[index].volatileStatuses = new List<StatusEffects>();

            UIManager.battleRects["Image_PlayerPokemon"].SetSprite(currentBattle.playerTeam[currentBattle.playerTeamIndex].backTexture);

            ((UISlider)UIManager.battleRects["Slider_PlayerHealth"]).maxValue = currentBattle.playerTeam[index].hpStat;
            ((UISlider)UIManager.battleRects["Slider_PlayerHealth"]).SetValue(currentBattle.playerTeam[index].currentHealth);

            ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).maxValue = currentBattle.playerTeam[index].maxExp;
            ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).SetValue(currentBattle.playerTeam[index].currentExp);

            ((UITextBox)UIManager.battleRects["Text_PlayerName"]).SetText(currentBattle.playerTeam[index].name.ToUpper());
            ((UITextBox)UIManager.battleRects["Text_PlayerLevel"]).SetText($"Lv{currentBattle.playerTeam[index].level}");

            ((UIButton)UIManager.battleRects["Button_Attack1"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack1"]).SetText("");
            ((UIButton)UIManager.battleRects["Button_Attack2"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack2"]).SetText("");
            ((UIButton)UIManager.battleRects["Button_Attack3"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack3"]).SetText("");
            ((UIButton)UIManager.battleRects["Button_Attack4"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack4"]).SetText("");

            if (currentBattle.playerTeam[index].moveset.Length > 0)
            {
                ((UIButton)UIManager.battleRects["Button_Attack1"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack1"]).SetText(currentBattle.playerTeam[index].moveset[0].name);
            }
            if (currentBattle.playerTeam[index].moveset.Length > 1)
            {
                ((UIButton)UIManager.battleRects["Button_Attack2"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack2"]).SetText(currentBattle.playerTeam[index].moveset[1].name);
            }
            if (currentBattle.playerTeam[index].moveset.Length > 2)
            {
                ((UIButton)UIManager.battleRects["Button_Attack3"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack3"]).SetText(currentBattle.playerTeam[index].moveset[2].name);
            }
            if (currentBattle.playerTeam[index].moveset.Length > 3)
            {
                ((UIButton)UIManager.battleRects["Button_Attack4"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack4"]).SetText(currentBattle.playerTeam[index].moveset[3].name);
            }
        }

        public static void Enemy_SwitchInPokemon(int index)
        {
            currentBattle.enemyTeamIndex = index;
            currentBattle.enemyTeam[index].volatileStatuses = new List<StatusEffects>();

            currentBattle.enemyTeam[index].volatileStatuses.Add(new StatusEffects(Global.random.Next(2, 6), VolatileStatusConditions.Confusion));

            ((UISlider)UIManager.battleRects["Slider_EnemyHealth"]).maxValue = currentBattle.enemyTeam[index].hpStat;
            ((UISlider)UIManager.battleRects["Slider_EnemyHealth"]).SetValue(currentBattle.enemyTeam[index].currentHealth);

            ((UITextBox)UIManager.battleRects["Text_EnemyName"]).SetText(currentBattle.enemyTeam[index].name.ToUpper());
            ((UITextBox)UIManager.battleRects["Text_EnemyLevel"]).SetText($"Lv{currentBattle.enemyTeam[index].level}");

            Debug.WriteLine($"{currentBattle.enemyTeam[index].currentHealth} / {currentBattle.enemyTeam[index].hpStat}");

            UIManager.battleRects["Image_EnemyPokemon"].SetSprite(currentBattle.enemyTeam[currentBattle.enemyTeamIndex].frontTexture);
        }
    }
}
