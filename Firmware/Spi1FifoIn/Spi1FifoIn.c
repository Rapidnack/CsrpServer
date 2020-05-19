#pragma NOIV               // Do not generate interrupt vectors

#include "fx2.h"
#include "fx2regs.h"
#include "syncdly.h"            // SYNCDELAY macro

extern BOOL GotSUD;             // Received setup data flag
extern BOOL Sleep;
extern BOOL Rwuen;
extern BOOL Selfpwr;

BYTE Configuration;             // Current configuration
BYTE AlternateSetting;          // Alternate settings

#define SPI_CS  PA7
#define SPI_CLK PA3
#define MISO    PA1
#define MOSI    PA0

//-----------------------------------------------------------------------------
// Task Dispatcher hooks
//   The following hooks are called by the task dispatcher.
//-----------------------------------------------------------------------------

BYTE SPIXferByte(BYTE b)
{
   // SPI mode 1

   BYTE val = 0x00;
   BYTE mask = 0x80;
   int i;

   for (i = 0; i < 8; i++)
   {
      if (b & mask) MOSI = 1; else MOSI = 0;
      SPI_CLK = 1;
      if (MISO == 1) val |= mask;
      SPI_CLK = 0;
      mask >>= 1;
   }
   return(val);
}

void TD_Init(void)             // Called once at startup
{
   CPUCS = ((CPUCS & ~bmCLKSPD) | bmCLKSPD1) ; // 48 MHz CPU clock

   REVCTL = 0x03; // REVCTL.0 and REVCTL.1 set to 1
   SYNCDELAY;


   EP1OUTCFG = 0xA0;
   EP1INCFG = 0xA0;

   // out endpoints do not come up armed
   EP1OUTBC = 0x00; // arm EP1OUT by writing any value
   SYNCDELAY;

   // bit-bang SPI
   OEA = 0x89; // 1xxx_1x01
   SPI_CS = 1;
   SPI_CLK = 0;
   MOSI = 0;


   EP2CFG = 0xE8; // IN, Bulk, 1024, Quad

   IFCONFIG = 0x43; // slave FIFO interface, external IFCLK

   FIFORESET = 0x80; // activate NAK-ALL to avoid race conditions
   SYNCDELAY;
   FIFORESET = 0x82; // Reset FIFO 2
   SYNCDELAY;
   FIFORESET = 0x00; // Release NAKALL
   SYNCDELAY;

   EP2FIFOCFG = 0x0C; // EP2 is AUTOOUT=0, AUTOIN=1, ZEROLEN=1, WORDWIDE=0
   SYNCDELAY;
   EP2AUTOINLENH = 0x04;
   SYNCDELAY;
   EP2AUTOINLENL = 0x00;
   SYNCDELAY;
}

void TD_Poll(void)              // Called repeatedly while the device is idle
{
   // bit-bang SPI
   if (!(EP01STAT & bmBIT1))
   {
      BYTE count = EP1OUTBC;
      BYTE i;

      SPI_CS = 0;
      for( i = 0; i < count; i++ )
      {
         EP1INBUF[i] = SPIXferByte(EP1OUTBUF[i]);
      }
      SPI_CS = 1;

      EP1INBC = EP1OUTBC;       // arm EP1IN
      EP1OUTBC = 0x00;          // re(arm) EP1OUT by writing any value
   }
}

BOOL TD_Suspend(void)          // Called before the device goes into suspend mode
{
   return(TRUE);
}

BOOL TD_Resume(void)          // Called after the device resumes
{
   return(TRUE);
}

//-----------------------------------------------------------------------------
// Device Request hooks
//   The following hooks are called by the end point 0 device request parser.
//-----------------------------------------------------------------------------

BOOL DR_GetDescriptor(void)
{
   return(TRUE);
}

BOOL DR_SetConfiguration(void)   // Called when a Set Configuration command is received
{
   Configuration = SETUPDAT[2];
   return(TRUE);            // Handled by user code
}

BOOL DR_GetConfiguration(void)   // Called when a Get Configuration command is received
{
   EP0BUF[0] = Configuration;
   EP0BCH = 0;
   EP0BCL = 1;
   return(TRUE);            // Handled by user code
}

BOOL DR_SetInterface(void)       // Called when a Set Interface command is received
{
   AlternateSetting = SETUPDAT[2];
   return(TRUE);            // Handled by user code
}

BOOL DR_GetInterface(void)       // Called when a Set Interface command is received
{
   EP0BUF[0] = AlternateSetting;
   EP0BCH = 0;
   EP0BCL = 1;
   return(TRUE);            // Handled by user code
}

BOOL DR_GetStatus(void)
{
   return(TRUE);
}

BOOL DR_ClearFeature(void)
{
   return(TRUE);
}

BOOL DR_SetFeature(void)
{
   return(TRUE);
}

BOOL DR_VendorCmnd(void)
{
   switch (SETUPDAT[1])
   {
      default:
         return(TRUE);
   }

   return(FALSE);
}

//-----------------------------------------------------------------------------
// USB Interrupt Handlers
//   The following functions are called by the USB interrupt jump table.
//-----------------------------------------------------------------------------

// Setup Data Available Interrupt Handler
void ISR_Sudav(void) interrupt 0
{
   GotSUD = TRUE;            // Set flag
   EZUSB_IRQ_CLEAR();
   USBIRQ = bmSUDAV;         // Clear SUDAV IRQ
}

// Setup Token Interrupt Handler
void ISR_Sutok(void) interrupt 0
{
   EZUSB_IRQ_CLEAR();
   USBIRQ = bmSUTOK;         // Clear SUTOK IRQ
}

void ISR_Sof(void) interrupt 0
{
   EZUSB_IRQ_CLEAR();
   USBIRQ = bmSOF;          	// Clear SOF IRQ
}

void ISR_Ures(void) interrupt 0
{
   // whenever we get a USB reset, we should revert to full speed mode
   pConfigDscr = pFullSpeedConfigDscr;
   ((CONFIGDSCR xdata *) pConfigDscr)->type = CONFIG_DSCR;
   pOtherConfigDscr = pHighSpeedConfigDscr;
   ((CONFIGDSCR xdata *) pOtherConfigDscr)->type = OTHERSPEED_DSCR;

   EZUSB_IRQ_CLEAR();
   USBIRQ = bmURES;         // Clear URES IRQ
}

void ISR_Susp(void) interrupt 0
{
   Sleep = TRUE;
   EZUSB_IRQ_CLEAR();
   USBIRQ = bmSUSP;
}

void ISR_Highspeed(void) interrupt 0
{
   if (EZUSB_HIGHSPEED())
   {
      pConfigDscr = pHighSpeedConfigDscr;
      ((CONFIGDSCR xdata *) pConfigDscr)->type = CONFIG_DSCR;
      pOtherConfigDscr = pFullSpeedConfigDscr;
      ((CONFIGDSCR xdata *) pOtherConfigDscr)->type = OTHERSPEED_DSCR;
   }

   EZUSB_IRQ_CLEAR();
   USBIRQ = bmHSGRANT;
}
void ISR_Ep0ack(void) interrupt 0
{
}
void ISR_Stub(void) interrupt 0
{
}
void ISR_Ep0in(void) interrupt 0
{
}
void ISR_Ep0out(void) interrupt 0
{
}
void ISR_Ep1in(void) interrupt 0
{
}
void ISR_Ep1out(void) interrupt 0
{
}
void ISR_Ep2inout(void) interrupt 0
{
}
void ISR_Ep4inout(void) interrupt 0
{
}
void ISR_Ep6inout(void) interrupt 0
{
}
void ISR_Ep8inout(void) interrupt 0
{
}
void ISR_Ibn(void) interrupt 0
{
}
void ISR_Ep0pingnak(void) interrupt 0
{
}
void ISR_Ep1pingnak(void) interrupt 0
{
}
void ISR_Ep2pingnak(void) interrupt 0
{
}
void ISR_Ep4pingnak(void) interrupt 0
{
}
void ISR_Ep6pingnak(void) interrupt 0
{
}
void ISR_Ep8pingnak(void) interrupt 0
{
}
void ISR_Errorlimit(void) interrupt 0
{
}
void ISR_Ep2piderror(void) interrupt 0
{
}
void ISR_Ep4piderror(void) interrupt 0
{
}
void ISR_Ep6piderror(void) interrupt 0
{
}
void ISR_Ep8piderror(void) interrupt 0
{
}
void ISR_Ep2pflag(void) interrupt 0
{
}
void ISR_Ep4pflag(void) interrupt 0
{
}
void ISR_Ep6pflag(void) interrupt 0
{
}
void ISR_Ep8pflag(void) interrupt 0
{
}
void ISR_Ep2eflag(void) interrupt 0
{
}
void ISR_Ep4eflag(void) interrupt 0
{
}
void ISR_Ep6eflag(void) interrupt 0
{
}
void ISR_Ep8eflag(void) interrupt 0
{
}
void ISR_Ep2fflag(void) interrupt 0
{
}
void ISR_Ep4fflag(void) interrupt 0
{
}
void ISR_Ep6fflag(void) interrupt 0
{
}
void ISR_Ep8fflag(void) interrupt 0
{
}
void ISR_GpifComplete(void) interrupt 0
{
}
void ISR_GpifWaveform(void) interrupt 0
{
}
