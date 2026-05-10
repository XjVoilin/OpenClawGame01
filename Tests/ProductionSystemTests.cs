using NUnit.Framework;
using IsleWorks.Systems;
using IsleWorks.Simulation;
using IsleWorks.Configs;
using UnityEngine;

namespace IsleWorks.Tests
{
    public class ProductionSystemTests
    {
        private ProductionSystem _productionSystem;
        private MachineInstance _multiInputMachine;

        [SetUp]
        public void Setup()
        {
            _productionSystem = new ProductionSystem();
            RecipeConfigLoader.LoadConfigs(); // 加载配方

            // 初始化多输入机器
            _multiInputMachine = new MachineInstance(1, 4)
            {
                CurrentRecipe = RecipeConfigLoader.GetRecipe(4) // 金属锭 + 塑料 → 机械组件
            };

            _multiInputMachine.InputSlots[0] = ResourceType.Ingot;
            _multiInputMachine.InputSlots[1] = ResourceType.Plastic;

            _productionSystem.RegisterMachine(_multiInputMachine);
        }

        [Test]
        public void MultiInputMachine_StartsProcessingWithCorrectInputs()
        {
            _productionSystem.OnUpdate(1.0f); // 模拟 1 秒
            Assert.IsTrue(_multiInputMachine.IsProcessing, "机器未正确启动加工流程");
            Assert.AreEqual(15f, _multiInputMachine.ProcessTimer, "未正确设置加工时间");
        }

        [Test]
        public void MultiInputMachine_CompletesProcessingAndProducesOutput()
        {
            _multiInputMachine.IsProcessing = true;
            _multiInputMachine.ProcessTimer = 1.0f;

            _productionSystem.OnUpdate(1.0f); // 模拟 1 秒

            Assert.IsFalse(_multiInputMachine.IsProcessing, "加工未结束");
            Assert.AreEqual(ResourceType.MechanicalComponent, _multiInputMachine.OutputSlot, "未正确产出机械组件");
        }

        [Test]
        public void MultiInputMachine_DoesNotStartProcessingWithIncompleteInputs()
        {
            _multiInputMachine.InputSlots[1] = ResourceType.None; // 移除一个输入

            _productionSystem.OnUpdate(1.0f); // 触发更新

            Assert.IsFalse(_multiInputMachine.IsProcessing, "不完整输入时不应启动加工");
        }
    }
}