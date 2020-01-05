#include <U8g2lib.h>

                                        //U8G2_R0为正向显示,U8G2_R2为旋转180度
U8G2_SSD1306_128X64_NONAME_F_HW_I2C u8g2(U8G2_R2, /* reset=*/U8X8_PIN_NONE);
//U8G2_SSD1306_128X64_NONAME_F_4W_HW_SPI u8g2(U8G2_R0, /* cs=*/ 4, /* dc=*/ 5, /* reset=*/ 3); //使用7个引脚SPI屏幕的取消注释这行并注释掉上一行
char Message[8192];
byte OneByte;

void Draw()
{
  u8g2.clearBuffer(); 
  for(int i=0;i<8192;i++)
  {
    if(Message[i]=='1')
    {
      int PixelXpos=i%128;
      int PixelYpos=i/128;
      u8g2.drawPixel(PixelXpos,PixelYpos);
    }
  }
  u8g2.sendBuffer();
}

void setup()
{
    // put your setup code here, to run once:
    u8g2.begin();
    Serial.begin(1500000);
}

void loop()
{
  u8g2.clearBuffer(); 
  if (Serial.available() > 0)
  {
    OneByte = Serial.read();
    if(OneByte == '@')
    {
      int i=0;
      while(OneByte!='&')
      {
        while (Serial.available() == 0)
                    ;
        OneByte=Serial.read();
        Message[i]=OneByte;
        i++;
      }

      //开始画图
      Draw();
    }
  }
}
