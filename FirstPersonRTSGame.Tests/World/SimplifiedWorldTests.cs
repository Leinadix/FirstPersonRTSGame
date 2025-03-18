using Xunit;
using Moq;
using System;
using System.Numerics;
using System.Collections.Generic;
using FirstPersonRTSGame.Game;
using FirstPersonRTSGame.Engine;

namespace FirstPersonRTSGame.Tests.World
{
    public class SimplifiedWorldTests
    {
        [Fact]
        public void TestWorld_UpdatesShips()
        {
            // Arrange
            var mockShip = new Mock<IShip>();
            var world = new TestWorld(new List<IShip> { mockShip.Object });
            float deltaTime = 0.16f;
            
            // Act
            world.Update(deltaTime);
            
            // Assert
            mockShip.Verify(s => s.Update(deltaTime), Times.Once);
        }
        
        [Fact]
        public void TestWorld_FindsClosestShip()
        {
            // Arrange
            var mockShip1 = new Mock<IShip>();
            mockShip1.Setup(s => s.Position).Returns(new Vector3(10, 0, 0));
            
            var mockShip2 = new Mock<IShip>();
            mockShip2.Setup(s => s.Position).Returns(new Vector3(20, 0, 0));
            
            var ships = new List<IShip> { mockShip1.Object, mockShip2.Object };
            var world = new TestWorld(ships);
            
            // Act
            var closestShip = world.GetClosestShip(Vector3.Zero, 100f);
            
            // Assert
            Assert.Same(mockShip1.Object, closestShip);
        }
        
        // Simple test implementation of World
        private class TestWorld : IWorld
        {
            private readonly List<IShip> _ships;
            
            public TestWorld(List<IShip> ships)
            {
                _ships = ships;
                Resources = new List<IResource>();
                Buildings = new List<IBuilding>();
                Ships = ships;
            }
            
            public float WaterLevel => 0f;
            public float TimeOfDay => 0f;
            public IEnumerable<IResource> Resources { get; }
            public IEnumerable<IBuilding> Buildings { get; }
            public IEnumerable<IShip> Ships { get; }
            
            public void Update(float deltaTime)
            {
                foreach (var ship in _ships)
                {
                    ship.Update(deltaTime);
                }
            }
            
            public float GetHeightAt(float x, float z)
            {
                return 0f;
            }
            
            public IShip GetClosestShip(Vector3 position, float maxDistance)
            {
                IShip closest = null;
                float closestDistance = maxDistance;
                
                foreach (var ship in _ships)
                {
                    float distance = Vector3.Distance(position, ship.Position);
                    if (distance < closestDistance)
                    {
                        closest = ship;
                        closestDistance = distance;
                    }
                }
                
                return closest;
            }
            
            public IBuilding GetClosestBuilding(Vector3 position, float maxDistance)
            {
                // Not implemented for these tests
                return null;
            }
            
            public IResource GetResourceInRange(Vector3 position, float maxDistance, ResourceType type)
            {
                // Not implemented for these tests
                return null;
            }
        }
    }
} 