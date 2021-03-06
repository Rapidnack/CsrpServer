`timescale 1 ps / 1 ps

module top
#(
	parameter LED_WIDTH = 4
)
(
	input wire CLK,
		
	input wire SPI_SCLK,
	input wire SPI_NSS,
	input wire SPI_MOSI,
	output wire SPI_MISO,
	
	input wire [7:0] ADC,
	output wire ENCODE,
	
	output wire IFCLK,
	output reg [7:0] FD,
	output wire SLRD,
	output wire SLWR,
	input wire [2:0] FLAG,
	output wire SLOE,
	output wire [1:0] FIFOADR,
	output wire PKTEND,
	
	output wire [LED_WIDTH-1:0] LED
);

	
	localparam CIC_NUM_STAGES = 4;
	localparam CIC_MAX_RATE = 160;
	localparam CIC_WIDTH = 19;
	localparam FIR_WIDTH = 16;

	
	wire [31:0] pio0;
	wire [31:0] pio1;
	
	wire [31:0] sample_data;	
	wire sample_valid;
	wire sample_ready;
	
	wire clk; // 64M
	
	wire [3:0] cic_rate_div;
	wire [4:0] cic_out_gain;
	wire [4:0] fir_out_gain;

	reg [7:0] uadc_r;
	wire signed [7:0] adc;
	
	wire signed [11:0] sin;
	wire signed [11:0] cos;
	
	reg signed [CIC_WIDTH-1:0] i;
	reg signed [CIC_WIDTH-1:0] q;

	wire signed [CIC_WIDTH-1:0] icic;
	wire signed [CIC_WIDTH-1:0] qcic;
	wire icic_valid;

	wire signed [FIR_WIDTH-1:0] ifir;
	wire signed [FIR_WIDTH-1:0] qfir;
	wire ifir_valid;

	
	pll	pll_inst (
		.inclk0 (CLK),
		.c0 (clk),
		.c1 (IFCLK) // 32M
	);
		
	QsysCore u0 (
		.clk_clk                                                                                         (clk),
		.reset_reset_n                                                                                   (1'b1),
		.spi_slave_to_avalon_mm_master_bridge_0_export_0_mosi_to_the_spislave_inst_for_spichain          (SPI_MOSI),
		.spi_slave_to_avalon_mm_master_bridge_0_export_0_nss_to_the_spislave_inst_for_spichain           (SPI_NSS),
		.spi_slave_to_avalon_mm_master_bridge_0_export_0_miso_to_and_from_the_spislave_inst_for_spichain (SPI_MISO),
		.spi_slave_to_avalon_mm_master_bridge_0_export_0_sclk_to_the_spislave_inst_for_spichain          (SPI_SCLK),
		.pio_0_external_connection_export                                                                (pio0),
		.pio_1_external_connection_export                                                                (pio1),
		.clk_0_clk                                                                                       (IFCLK),
		.reset_0_reset_n                                                                                 (1'b1),
		.dc_fifo_0_in_data                                                                               (sample_data),
		.dc_fifo_0_in_valid                                                                              (sample_valid),
		.dc_fifo_0_in_ready                                                                              (sample_ready),
		.mysendtofx2lp_0_fx2lp_fd                                                                        (FD),                                                                          //                             sendtofx2lp_0_fx2lp.fd
		.mysendtofx2lp_0_fx2lp_slrd_n                                                                    (SLRD),                                                                      //                                                .slrd_n
		.mysendtofx2lp_0_fx2lp_slwr_n                                                                    (SLWR),                                                                      //                                                .slwr_n
		.mysendtofx2lp_0_fx2lp_flag_n                                                                    (FLAG),                                                                     //                                                .flagb_n
		.mysendtofx2lp_0_fx2lp_sloe_n                                                                    (SLOE),                                                                      //                                                .sloe_n
		.mysendtofx2lp_0_fx2lp_fifoadr                                                                   (FIFOADR),                                                                     //                                                .fifoadr
		.mysendtofx2lp_0_fx2lp_pktend_n                                                                  (PKTEND)                                                                     //                                                .pktend_n
	);

	assign LED = ~pio0[LED_WIDTH-1:0];
	
	assign cic_rate_div = pio0[11:8];
	assign cic_out_gain = pio0[16:12];
	assign fir_out_gain = pio0[21:17];

	always @(posedge clk) begin
		uadc_r <= ADC;
	end
	assign ENCODE = clk;
	assign adc = (uadc_r[7] == 0) ? uadc_r + 8'h80 : uadc_r - 8'h80;

	MyNCO #(
		.OUT_WIDTH(12)
	) nco_inst (
		.clk       (clk),
		.reset_n   (1'b1),
		.clken     (1'b1),
		.phi_inc_i (pio1),
		.fsin_o    (sin),
		.fcos_o    (cos),
		.out_valid ()
	);

	always @(posedge clk) begin
		i <= adc * cos;
		q <= adc * -sin;
	end
	
	MyCIC #(
		.NUM_STAGES(CIC_NUM_STAGES),
		.MAX_RATE(CIC_MAX_RATE),
		.DATA_WIDTH(CIC_WIDTH)
	) cic_inst_i (
		.clk       (clk),
		.reset_n   (1'b1),
		.rate      (CIC_MAX_RATE >> cic_rate_div),
		.rate_div  (cic_rate_div),
		.gain      (cic_out_gain),
		.in_error  (2'b00),
		.in_valid  (1'b1),
		.in_ready  (),
		.in_data   (i),
		.out_data  (icic),
		.out_error (),
		.out_valid (icic_valid),
		.out_ready (1'b1)
	);

	MyCIC #(
		.NUM_STAGES(CIC_NUM_STAGES),
		.MAX_RATE(CIC_MAX_RATE),
		.DATA_WIDTH(CIC_WIDTH)
	) cic_inst_q (
		.clk       (clk),
		.reset_n   (1'b1),
		.rate      (CIC_MAX_RATE >> cic_rate_div),
		.rate_div  (cic_rate_div),
		.gain      (cic_out_gain),
		.in_error  (2'b00),
		.in_valid  (1'b1),
		.in_ready  (),
		.in_data   (q),
		.out_data  (qcic),
		.out_error (),
		.out_valid (),
		.out_ready (1'b1)
	);
  
	MyFIR #(
		.DATA_WIDTH(FIR_WIDTH)
	) fir8_inst_i (
		.clk       (clk),
		.reset_n   (1'b1),
		.gain      (fir_out_gain),
		.ast_sink_data (icic[CIC_WIDTH-1 -: FIR_WIDTH]),
		.ast_sink_valid (icic_valid),
		.ast_sink_error (2'b00),
		.ast_source_data (ifir),
		.ast_source_valid (ifir_valid),
		.ast_source_error ()
	);
 
	MyFIR #(
		.DATA_WIDTH(FIR_WIDTH)
	) fir8_inst_q (
		.clk       (clk),
		.reset_n   (1'b1),
		.gain      (fir_out_gain),
		.ast_sink_data (qcic[CIC_WIDTH-1 -: FIR_WIDTH]),
		.ast_sink_valid (icic_valid),
		.ast_sink_error (2'b00),
		.ast_source_data (qfir),
		.ast_source_valid (),
		.ast_source_error ()
	);
	
	assign sample_data = { qfir, ifir };
	assign sample_valid = ifir_valid;
	
endmodule