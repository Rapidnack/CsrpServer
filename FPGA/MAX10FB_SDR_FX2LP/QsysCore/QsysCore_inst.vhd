	component QsysCore is
		port (
			clk_clk                                                                                         : in    std_logic                     := 'X';             -- clk
			clk_0_clk                                                                                       : in    std_logic                     := 'X';             -- clk
			dc_fifo_0_in_data                                                                               : in    std_logic_vector(31 downto 0) := (others => 'X'); -- data
			dc_fifo_0_in_valid                                                                              : in    std_logic                     := 'X';             -- valid
			dc_fifo_0_in_ready                                                                              : out   std_logic;                                        -- ready
			mysendtofx2lp_0_fx2lp_fd                                                                        : out   std_logic_vector(7 downto 0);                     -- fd
			mysendtofx2lp_0_fx2lp_slrd_n                                                                    : out   std_logic;                                        -- slrd_n
			mysendtofx2lp_0_fx2lp_slwr_n                                                                    : out   std_logic;                                        -- slwr_n
			mysendtofx2lp_0_fx2lp_sloe_n                                                                    : out   std_logic;                                        -- sloe_n
			mysendtofx2lp_0_fx2lp_fifoadr                                                                   : out   std_logic_vector(1 downto 0);                     -- fifoadr
			mysendtofx2lp_0_fx2lp_pktend_n                                                                  : out   std_logic;                                        -- pktend_n
			mysendtofx2lp_0_fx2lp_flag_n                                                                    : in    std_logic_vector(2 downto 0)  := (others => 'X'); -- flag_n
			pio_0_external_connection_export                                                                : out   std_logic_vector(31 downto 0);                    -- export
			pio_1_external_connection_export                                                                : out   std_logic_vector(31 downto 0);                    -- export
			reset_reset_n                                                                                   : in    std_logic                     := 'X';             -- reset_n
			reset_0_reset_n                                                                                 : in    std_logic                     := 'X';             -- reset_n
			spi_slave_to_avalon_mm_master_bridge_0_export_0_mosi_to_the_spislave_inst_for_spichain          : in    std_logic                     := 'X';             -- mosi_to_the_spislave_inst_for_spichain
			spi_slave_to_avalon_mm_master_bridge_0_export_0_nss_to_the_spislave_inst_for_spichain           : in    std_logic                     := 'X';             -- nss_to_the_spislave_inst_for_spichain
			spi_slave_to_avalon_mm_master_bridge_0_export_0_miso_to_and_from_the_spislave_inst_for_spichain : inout std_logic                     := 'X';             -- miso_to_and_from_the_spislave_inst_for_spichain
			spi_slave_to_avalon_mm_master_bridge_0_export_0_sclk_to_the_spislave_inst_for_spichain          : in    std_logic                     := 'X'              -- sclk_to_the_spislave_inst_for_spichain
		);
	end component QsysCore;

	u0 : component QsysCore
		port map (
			clk_clk                                                                                         => CONNECTED_TO_clk_clk,                                                                                         --                                             clk.clk
			clk_0_clk                                                                                       => CONNECTED_TO_clk_0_clk,                                                                                       --                                           clk_0.clk
			dc_fifo_0_in_data                                                                               => CONNECTED_TO_dc_fifo_0_in_data,                                                                               --                                    dc_fifo_0_in.data
			dc_fifo_0_in_valid                                                                              => CONNECTED_TO_dc_fifo_0_in_valid,                                                                              --                                                .valid
			dc_fifo_0_in_ready                                                                              => CONNECTED_TO_dc_fifo_0_in_ready,                                                                              --                                                .ready
			mysendtofx2lp_0_fx2lp_fd                                                                        => CONNECTED_TO_mysendtofx2lp_0_fx2lp_fd,                                                                        --                           mysendtofx2lp_0_fx2lp.fd
			mysendtofx2lp_0_fx2lp_slrd_n                                                                    => CONNECTED_TO_mysendtofx2lp_0_fx2lp_slrd_n,                                                                    --                                                .slrd_n
			mysendtofx2lp_0_fx2lp_slwr_n                                                                    => CONNECTED_TO_mysendtofx2lp_0_fx2lp_slwr_n,                                                                    --                                                .slwr_n
			mysendtofx2lp_0_fx2lp_sloe_n                                                                    => CONNECTED_TO_mysendtofx2lp_0_fx2lp_sloe_n,                                                                    --                                                .sloe_n
			mysendtofx2lp_0_fx2lp_fifoadr                                                                   => CONNECTED_TO_mysendtofx2lp_0_fx2lp_fifoadr,                                                                   --                                                .fifoadr
			mysendtofx2lp_0_fx2lp_pktend_n                                                                  => CONNECTED_TO_mysendtofx2lp_0_fx2lp_pktend_n,                                                                  --                                                .pktend_n
			mysendtofx2lp_0_fx2lp_flag_n                                                                    => CONNECTED_TO_mysendtofx2lp_0_fx2lp_flag_n,                                                                    --                                                .flag_n
			pio_0_external_connection_export                                                                => CONNECTED_TO_pio_0_external_connection_export,                                                                --                       pio_0_external_connection.export
			pio_1_external_connection_export                                                                => CONNECTED_TO_pio_1_external_connection_export,                                                                --                       pio_1_external_connection.export
			reset_reset_n                                                                                   => CONNECTED_TO_reset_reset_n,                                                                                   --                                           reset.reset_n
			reset_0_reset_n                                                                                 => CONNECTED_TO_reset_0_reset_n,                                                                                 --                                         reset_0.reset_n
			spi_slave_to_avalon_mm_master_bridge_0_export_0_mosi_to_the_spislave_inst_for_spichain          => CONNECTED_TO_spi_slave_to_avalon_mm_master_bridge_0_export_0_mosi_to_the_spislave_inst_for_spichain,          -- spi_slave_to_avalon_mm_master_bridge_0_export_0.mosi_to_the_spislave_inst_for_spichain
			spi_slave_to_avalon_mm_master_bridge_0_export_0_nss_to_the_spislave_inst_for_spichain           => CONNECTED_TO_spi_slave_to_avalon_mm_master_bridge_0_export_0_nss_to_the_spislave_inst_for_spichain,           --                                                .nss_to_the_spislave_inst_for_spichain
			spi_slave_to_avalon_mm_master_bridge_0_export_0_miso_to_and_from_the_spislave_inst_for_spichain => CONNECTED_TO_spi_slave_to_avalon_mm_master_bridge_0_export_0_miso_to_and_from_the_spislave_inst_for_spichain, --                                                .miso_to_and_from_the_spislave_inst_for_spichain
			spi_slave_to_avalon_mm_master_bridge_0_export_0_sclk_to_the_spislave_inst_for_spichain          => CONNECTED_TO_spi_slave_to_avalon_mm_master_bridge_0_export_0_sclk_to_the_spislave_inst_for_spichain           --                                                .sclk_to_the_spislave_inst_for_spichain
		);

