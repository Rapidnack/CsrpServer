
module QsysCore (
	clk_clk,
	clk_0_clk,
	dc_fifo_0_in_data,
	dc_fifo_0_in_valid,
	dc_fifo_0_in_ready,
	mysendtofx2lp_0_fx2lp_fd,
	mysendtofx2lp_0_fx2lp_slrd_n,
	mysendtofx2lp_0_fx2lp_slwr_n,
	mysendtofx2lp_0_fx2lp_sloe_n,
	mysendtofx2lp_0_fx2lp_fifoadr,
	mysendtofx2lp_0_fx2lp_pktend_n,
	mysendtofx2lp_0_fx2lp_flag_n,
	pio_0_external_connection_export,
	pio_1_external_connection_export,
	reset_reset_n,
	reset_0_reset_n,
	spi_slave_to_avalon_mm_master_bridge_0_export_0_mosi_to_the_spislave_inst_for_spichain,
	spi_slave_to_avalon_mm_master_bridge_0_export_0_nss_to_the_spislave_inst_for_spichain,
	spi_slave_to_avalon_mm_master_bridge_0_export_0_miso_to_and_from_the_spislave_inst_for_spichain,
	spi_slave_to_avalon_mm_master_bridge_0_export_0_sclk_to_the_spislave_inst_for_spichain);	

	input		clk_clk;
	input		clk_0_clk;
	input	[31:0]	dc_fifo_0_in_data;
	input		dc_fifo_0_in_valid;
	output		dc_fifo_0_in_ready;
	output	[7:0]	mysendtofx2lp_0_fx2lp_fd;
	output		mysendtofx2lp_0_fx2lp_slrd_n;
	output		mysendtofx2lp_0_fx2lp_slwr_n;
	output		mysendtofx2lp_0_fx2lp_sloe_n;
	output	[1:0]	mysendtofx2lp_0_fx2lp_fifoadr;
	output		mysendtofx2lp_0_fx2lp_pktend_n;
	input	[2:0]	mysendtofx2lp_0_fx2lp_flag_n;
	output	[31:0]	pio_0_external_connection_export;
	output	[31:0]	pio_1_external_connection_export;
	input		reset_reset_n;
	input		reset_0_reset_n;
	input		spi_slave_to_avalon_mm_master_bridge_0_export_0_mosi_to_the_spislave_inst_for_spichain;
	input		spi_slave_to_avalon_mm_master_bridge_0_export_0_nss_to_the_spislave_inst_for_spichain;
	inout		spi_slave_to_avalon_mm_master_bridge_0_export_0_miso_to_and_from_the_spislave_inst_for_spichain;
	input		spi_slave_to_avalon_mm_master_bridge_0_export_0_sclk_to_the_spislave_inst_for_spichain;
endmodule
