using NUnit.Framework;
using IsleWorks;
using IsleWorks.Production;
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

            _multiInputMachine = new MachineInstance(1, 4, Vector2Int.zero, Vector2Int.one, 2);
            _multiInputMachine.InputSlots[0] = ResourceType.Ingot;
            _multiInputMachine.InputSlots[1] = ResourceType.Plastic;
        }

        [Test]
        public void MultiInputMachine_StartsProcessingWithCorrectInputs()
        {
            _productionSystem.OnUpdate(1.0f);
            Assert.IsTrue(_multiInputMachine.IsProcessing, "机器未正确启动加工流程");
            Assert.AreEqual(15f, _multiInputMachine.ProcessTimer, "未正确设置加工时间");
        }

        [Test]
        public void MultiInputMachine_CompletesProcessingAndProducesOutput()
        {
            _multiInputMachine.IsProcessing = true;
            _multiInputMachine.ProcessTimer = 1.0f;

            _productionSystem.OnUpdate(1.0f);

            Assert.IsFalse(_multiInputMachine.IsProcessing, "加工未结束");
            Assert.AreEqual(ResourceType.MechanicalComponent, _multiInputMachine.OutputSlot, "未正确产出机械组件");
        }

        [Test]
        public void MultiInputMachine_DoesNotStartProcessingWithIncompleteInputs()
        {
            _multiInputMachine.InputSlots[1] = ResourceType.None;

            _productionSystem.OnUpdate(1.0f);

            Assert.IsFalse(_multiInputMachine.IsProcessing, "不完整输入时不应启动加工");
        }
    }
}
