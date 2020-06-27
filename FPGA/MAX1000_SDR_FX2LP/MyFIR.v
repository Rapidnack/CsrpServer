`timescale 1ns/1ns


module MyFIR
#(
	parameter RATE = 8,
	parameter DATA_WIDTH = 16
)
(
	input wire clk,
	input wire reset_n,
	
	input wire [4:0] gain,
	
	input wire signed [DATA_WIDTH-1:0] ast_sink_data,
	input wire ast_sink_valid,
	input wire [1:0] ast_sink_error,

	output reg signed [DATA_WIDTH-1:0] ast_source_data,
	output reg ast_source_valid,
	output wire [1:0] ast_source_error
);


	localparam NUM_TAPS = 155;
	localparam H_WIDTH = 16;
	localparam Y_WIDTH = H_WIDTH + DATA_WIDTH - 1;

	// RAM 16x256
	localparam RAM_NUM_ADDRS = 256;
	
	
	integer i;

	reg signed [H_WIDTH-1:0] h[0:NUM_TAPS-1];
	wire signed [DATA_WIDTH-1:0] x0;
	wire signed [DATA_WIDTH-1:0] x1;
	wire signed [DATA_WIDTH-1:0] x2;
	wire signed [DATA_WIDTH-1:0] x3;
	reg signed [Y_WIDTH-1:0] y;
	
	wire [7:0] raddr;
	reg [7:0] waddr;
	reg [7:0] last_waddr;

	reg [8:0] cnt;
	reg [7:0] deci_cnt;

	
	// Normalized Frequency: 0.0625
	initial begin
		h[0] = -10;
		h[1] = -11;
		h[2] = -10;
		h[3] = -8;
		h[4] = -5;
		h[5] = 0;
		h[6] = 5;
		h[7] = 10;
		h[8] = 15;
		h[9] = 17;
		h[10] = 17;
		h[11] = 14;
		h[12] = 8;
		h[13] = 0;
		h[14] = -10;
		h[15] = -19;
		h[16] = -28;
		h[17] = -32;
		h[18] = -32;
		h[19] = -27;
		h[20] = -16;
		h[21] = 0;
		h[22] = 18;
		h[23] = 37;
		h[24] = 52;
		h[25] = 60;
		h[26] = 60;
		h[27] = 49;
		h[28] = 28;
		h[29] = 0;
		h[30] = -33;
		h[31] = -64;
		h[32] = -90;
		h[33] = -104;
		h[34] = -102;
		h[35] = -83;
		h[36] = -48;
		h[37] = 0;
		h[38] = 54;
		h[39] = 107;
		h[40] = 148;
		h[41] = 170;
		h[42] = 167;
		h[43] = 135;
		h[44] = 78;
		h[45] = 0;
		h[46] = -87;
		h[47] = -171;
		h[48] = -237;
		h[49] = -272;
		h[50] = -267;
		h[51] = -217;
		h[52] = -125;
		h[53] = 0;
		h[54] = 141;
		h[55] = 277;
		h[56] = 386;
		h[57] = 446;
		h[58] = 441;
		h[59] = 361;
		h[60] = 210;
		h[61] = 0;
		h[62] = -244;
		h[63] = -488;
		h[64] = -694;
		h[65] = -822;
		h[66] = -836;
		h[67] = -710;
		h[68] = -430;
		h[69] = 0;
		h[70] = 560;
		h[71] = 1212;
		h[72] = 1909;
		h[73] = 2592;
		h[74] = 3201;
		h[75] = 3682;
		h[76] = 3990;
		h[77] = 4096;
		h[78] = 3990;
		h[79] = 3682;
		h[80] = 3201;
		h[81] = 2592;
		h[82] = 1909;
		h[83] = 1212;
		h[84] = 560;
		h[85] = 0;
		h[86] = -430;
		h[87] = -710;
		h[88] = -836;
		h[89] = -822;
		h[90] = -694;
		h[91] = -488;
		h[92] = -244;
		h[93] = 0;
		h[94] = 210;
		h[95] = 361;
		h[96] = 441;
		h[97] = 446;
		h[98] = 386;
		h[99] = 277;
		h[100] = 141;
		h[101] = 0;
		h[102] = -125;
		h[103] = -217;
		h[104] = -267;
		h[105] = -272;
		h[106] = -237;
		h[107] = -171;
		h[108] = -87;
		h[109] = 0;
		h[110] = 78;
		h[111] = 135;
		h[112] = 167;
		h[113] = 170;
		h[114] = 148;
		h[115] = 107;
		h[116] = 54;
		h[117] = 0;
		h[118] = -48;
		h[119] = -83;
		h[120] = -102;
		h[121] = -104;
		h[122] = -90;
		h[123] = -64;
		h[124] = -33;
		h[125] = 0;
		h[126] = 28;
		h[127] = 49;
		h[128] = 60;
		h[129] = 60;
		h[130] = 52;
		h[131] = 37;
		h[132] = 18;
		h[133] = 0;
		h[134] = -16;
		h[135] = -27;
		h[136] = -32;
		h[137] = -32;
		h[138] = -28;
		h[139] = -19;
		h[140] = -10;
		h[141] = 0;
		h[142] = 8;
		h[143] = 14;
		h[144] = 17;
		h[145] = 17;
		h[146] = 15;
		h[147] = 10;
		h[148] = 5;
		h[149] = 0;
		h[150] = -5;
		h[151] = -8;
		h[152] = -10;
		h[153] = -11;
		h[154] = -10;
	end

	
	assign ast_source_error = 2'b00;
	
	always @(posedge clk)
	begin
		if (~reset_n) begin
			waddr <= 0;
			cnt <= 0;
			
			deci_cnt <= 0;
			
			y <= 0;
			ast_source_data <= 0;
			ast_source_valid <= 1'b0;
		end
		else begin
			
			if (ast_sink_valid) begin			
				if (waddr == RAM_NUM_ADDRS-1) begin
					waddr <= 0;
				end
				else begin
					waddr <= waddr + 1;			
				end
								
				if (deci_cnt == RATE-1) begin
					deci_cnt <= 0;
					
					ast_source_data <= y[Y_WIDTH-1-gain -: DATA_WIDTH];
					ast_source_valid <= 1'b1;
					
					last_waddr <= waddr;
					cnt <= 0;
					y <= 0;
				end
				else begin
					deci_cnt <= deci_cnt + 1;
					
					if (cnt < NUM_TAPS) begin
						cnt <= cnt + 4;
					end
					if (cnt + 3 < NUM_TAPS) begin
						y <= y + h[cnt] * x0 + h[cnt + 1] * x1 + h[cnt + 2] * x2 + h[cnt + 3] * x3;
					end
					else if (cnt + 2 < NUM_TAPS) begin
						y <= y + h[cnt] * x0 + h[cnt + 1] * x1 + h[cnt + 2] * x2;
					end
					else if (cnt + 1 < NUM_TAPS) begin
						y <= y + h[cnt] * x0 + h[cnt + 1] * x1;
					end
					else if (cnt < NUM_TAPS) begin
						y <= y + h[cnt] * x0;
					end
					ast_source_valid <= 1'b0;
				end
			end
			else begin
				if (cnt < NUM_TAPS) begin
					cnt <= cnt + 4;
				end
				if (cnt + 3 < NUM_TAPS) begin
					y <= y + h[cnt] * x0 + h[cnt + 1] * x1 + h[cnt + 2] * x2 + h[cnt + 3] * x3;
				end
				else if (cnt + 2 < NUM_TAPS) begin
					y <= y + h[cnt] * x0 + h[cnt + 1] * x1 + h[cnt + 2] * x2;
				end
				else if (cnt + 1 < NUM_TAPS) begin
					y <= y + h[cnt] * x0 + h[cnt + 1] * x1;
				end
				else if (cnt < NUM_TAPS) begin
					y <= y + h[cnt] * x0;
				end
				ast_source_valid <= 1'b0;
			end
		end
	end
	
	assign raddr = (RAM_NUM_ADDRS + last_waddr - (NUM_TAPS - 1) + cnt <= RAM_NUM_ADDRS-1)
					? RAM_NUM_ADDRS + last_waddr - (NUM_TAPS - 1) + cnt
					: RAM_NUM_ADDRS + last_waddr - (NUM_TAPS - 1) + cnt - RAM_NUM_ADDRS;

	MyRAM16x256 ram16x256_inst (
		.clock (clk),
		.data (ast_sink_data),
		.rdaddress (raddr),
		.wraddress (waddr),
		.wren (ast_sink_valid),
		.q0 (x0),
		.q1 (x1),
		.q2 (x2),
		.q3 (x3)
	);


endmodule