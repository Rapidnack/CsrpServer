`timescale 1ns/1ns


module MyCIC
 #(
	parameter NUM_STAGES = 4,
	parameter MAX_RATE = 160,
	parameter DATA_WIDTH = 19
 )
(
	input wire clk,
	input wire reset_n,
	
	input wire [7:0] rate,
	input wire [3:0] rate_div,
	input wire [4:0] gain,
	
	input wire [1:0] in_error,
	input wire in_valid,
	output wire in_ready,
	input wire signed [DATA_WIDTH-1:0] in_data,

	output reg signed [DATA_WIDTH-1:0] out_data,
	output wire [1:0] out_error,
	output reg out_valid,
	input wire out_ready
);

	
	function integer clog2;
		input integer value;
		begin
			value = value - 1;
			for (clog2 = 0; value > 0; clog2 = clog2 + 1)
				value = value >> 1;
		end
	endfunction

	
	localparam RATE_WIDTH = clog2(MAX_RATE);
	localparam SCALE_WIDTH = clog2(MAX_RATE**NUM_STAGES);
	localparam WIDTH = SCALE_WIDTH + DATA_WIDTH;

	
	integer i;

	reg signed [WIDTH-1:0] integ [0:NUM_STAGES];
	reg signed [WIDTH-1:0] diff [0:NUM_STAGES];
	reg signed [WIDTH-1:0] diff_d [0:NUM_STAGES];

	reg [RATE_WIDTH-1:0] count;
	reg next_out_valid;

	
	assign in_ready = 1'b1;
	assign out_error = 2'b00;
	
	always @(posedge clk)
	begin
		if (~reset_n)
		begin
			for (i = 1; i <= NUM_STAGES; i = i + 1) begin
				integ[i] <= 0;
			end
			
			diff[1] <= 0;
		end else if (in_valid)
		begin
			integ[1] <= in_data + integ[1];
			
			for (i = 2; i <= NUM_STAGES; i = i + 1) begin
				integ[i] <= integ[i-1] + integ[i];
			end
			
			diff[1] <= integ[NUM_STAGES-1] + integ[NUM_STAGES];
		end
	end
	
	always @(posedge clk)
	begin
		if (~reset_n)
		begin
			count <= 0;
			next_out_valid <= 1'b1;
		end else if (in_valid)
		begin			
			if (count == rate - 1)
			begin
				count <= 0;
				next_out_valid <= 1'b1;
			end else
			begin
				count <= count + 1;
				next_out_valid <= 1'b0;
			end
		end else
		begin
			next_out_valid <= 1'b0;
		end
	end
	
	always @(posedge clk)
	begin
		if (~reset_n)
		begin
			for (i = 1; i <= NUM_STAGES; i = i + 1) begin
				diff_d[i] <= 0;
			end
			
			for (i = 2; i <= NUM_STAGES; i = i + 1) begin
				diff[i] <= 0;
			end
			
			out_data <= 0;
		end else
		begin
			out_valid <= next_out_valid;
		
			if (next_out_valid)
			begin
				for (i = 1; i <= NUM_STAGES; i = i + 1) begin
					diff_d[i] <= diff[i];
				end
			
				for (i = 2; i <= NUM_STAGES; i = i + 1) begin
					diff[i] <= diff[i-1] - diff_d[i-1];
				end
								
				out_data <= (diff[NUM_STAGES] - diff_d[NUM_STAGES]) >>> (SCALE_WIDTH - rate_div * NUM_STAGES + 8 - gain);
			end
		end
	end
	
endmodule