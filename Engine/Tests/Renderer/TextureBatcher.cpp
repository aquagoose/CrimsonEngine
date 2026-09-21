#include "TestFramework.h"

using namespace cge;

class HelloWorldTest : public TestBase
{
    std::unique_ptr<Texture> _texture1;
    std::unique_ptr<Texture> _texture2;

    f32 _value{};

public:
    HelloWorldTest() : TestBase("Hello World Test") {}

    void Init() override
    {
        _texture1 = Renderer->CreateTexture("Content/DEBUG.png");
        _texture2 = Renderer->CreateTexture("Content/Bagel.png");
    }

    void Loop(f32 dt) override
    {
        _value += 1.0f / 60.0f;
        if (_value >= 2 * M_PI)
            _value -= 2 * M_PI;

        for (int i = 0; i < 10; i++)
        {
            float v = std::sin(_value + i) * 100;
            Renderer->DrawImage(*_texture1, Vec2f(v, static_cast<float>(i * 50)));
        }

        for (int i = 0; i < 10; i++)
        {
            float v = std::cos(_value + i) * 100;
            Renderer->DrawImage(*_texture2, Vec2f(600 - v, i * 50), Color::Aquamarine());
        }
    }
};

int main(int argc, char* argv[])
{
    HelloWorldTest test;
    test.Run();

    return 0;
}
