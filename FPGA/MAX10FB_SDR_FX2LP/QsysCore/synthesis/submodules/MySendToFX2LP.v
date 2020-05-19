`timescale 1 ps / 1 ps

module MySendToFX2LP (
		input  wire        csi_clk,       //      csi.clk
		input  wire        rsi_reset,     //      rsi.reset

		input  wire [31:0] asi_in0_data,  //  asi_in0.data
		input  wire        asi_in0_valid, //         .valid
		output wire        asi_in0_ready, //         .ready
		
		output reg  [7:0]  coe_fx2lp_fd,
		output wire        coe_fx2lp_slrd_n,
		output wire        coe_fx2lp_slwr_n,
		input  wire [2:0]  coe_fx2lp_flag_n,
		output wire        coe_fx2lp_sloe_n,
		output wire [1:0]  coe_fx2lp_fifoadr,
		output wire        coe_fx2lp_pktend_n
);


	localparam IDLE = 3'd0;
	localparam BYTE0 = 3'd1;
	localparam BYTE1 = 3'd2;
	localparam BYTE2 = 3'd3;
	localparam BYTE3 = 3'd4;
	localparam BYTE4 = 3'd5;
	
	
	reg [2:0] cur;
	reg [31:0] asi_in0_data_r;

	
	assign coe_fx2lp_slrd_n = 1'b1;
	assign coe_fx2lp_sloe_n = 1'b1;
	assign coe_fx2lp_fifoadr = 2'b00;
	assign coe_fx2lp_pktend_n = 1'b1;
		
	always @(negedge csi_clk) begin
		if (rsi_reset) begin
			cur <= IDLE;
			coe_fx2lp_fd <= 0;
		end else begin
			case (cur)
				IDLE:
					if (asi_in0_valid & asi_in0_ready) begin
						cur <= BYTE0;
						coe_fx2lp_fd <= asi_in0_data[7:0];
						asi_in0_data_r <= asi_in0_data;
					end else begin
						cur <= IDLE;
					end
				BYTE0:
					begin
						cur <= BYTE1;
						coe_fx2lp_fd <= asi_in0_data_r[15:8];
					end
				BYTE1:
					begin
						cur <= BYTE2;
						coe_fx2lp_fd <= asi_in0_data_r[23:16];
					end
				BYTE2:
					begin
						cur <= BYTE3;
						coe_fx2lp_fd <= asi_in0_data_r[31:24];
					end
				BYTE3: cur <= IDLE;
				default: cur <= IDLE;
			endcase
		end
	end
	assign coe_fx2lp_slwr_n = cur < BYTE0 || BYTE3 < cur;
	assign asi_in0_ready = coe_fx2lp_flag_n[1] & coe_fx2lp_slwr_n;
	
endmodule
