module MyRAM16x256 (
	input wire clock,
	input wire [15:0] data,
	input wire [7:0] rdaddress,
	input wire [7:0] wraddress,
	input wire wren,
	output wire [15:0] q0,
	output wire [15:0] q1,
	output wire [15:0] q2,
	output wire [15:0] q3
);

	wire wren_ram0;
	wire wren_ram1;
	wire wren_ram2;
	wire wren_ram3;

	wire [7:0] rdaddress1;
	wire [7:0] rdaddress2;
	wire [7:0] rdaddress3;

	wire [7:0] rdaddress_ram0;
	wire [7:0] rdaddress_ram1;
	wire [7:0] rdaddress_ram2;
	wire [7:0] rdaddress_ram3;

	wire [15:0] q_ram0;
	wire [15:0] q_ram1;
	wire [15:0] q_ram2;
	wire [15:0] q_ram3;

	assign wren_ram0 = wren & (wraddress[1:0] == 2'b00);
	assign wren_ram1 = wren & (wraddress[1:0] == 2'b01);
	assign wren_ram2 = wren & (wraddress[1:0] == 2'b10);
	assign wren_ram3 = wren & (wraddress[1:0] == 2'b11);

	assign rdaddress1 = rdaddress + 1;
	assign rdaddress2 = rdaddress + 2;
	assign rdaddress3 = rdaddress + 3;

	assign rdaddress_ram0 = rdaddress[1:0] == 2'b00 ? rdaddress :
									rdaddress[1:0] == 2'b01 ? rdaddress3 :
									rdaddress[1:0] == 2'b10 ? rdaddress2 : rdaddress1;

	assign rdaddress_ram1 = rdaddress[1:0] == 2'b00 ? rdaddress1 :
									rdaddress[1:0] == 2'b01 ? rdaddress :
									rdaddress[1:0] == 2'b10 ? rdaddress3 : rdaddress2;

	assign rdaddress_ram2 = rdaddress[1:0] == 2'b00 ? rdaddress2 :
									rdaddress[1:0] == 2'b01 ? rdaddress1 :
									rdaddress[1:0] == 2'b10 ? rdaddress : rdaddress3;

	assign rdaddress_ram3 = rdaddress[1:0] == 2'b00 ? rdaddress3 :
									rdaddress[1:0] == 2'b01 ? rdaddress2 :
									rdaddress[1:0] == 2'b10 ? rdaddress1 : rdaddress;

	assign q0 = rdaddress[1:0] == 2'b00 ? q_ram0 :
				   rdaddress[1:0] == 2'b01 ? q_ram1 :
				   rdaddress[1:0] == 2'b10 ? q_ram2 : q_ram3;

	assign q1 = rdaddress1[1:0] == 2'b00 ? q_ram0 :
				   rdaddress1[1:0] == 2'b01 ? q_ram1 :
				   rdaddress1[1:0] == 2'b10 ? q_ram2 : q_ram3;

	assign q2 = rdaddress2[1:0] == 2'b00 ? q_ram0 :
				   rdaddress2[1:0] == 2'b01 ? q_ram1 :
				   rdaddress2[1:0] == 2'b10 ? q_ram2 : q_ram3;

	assign q3 = rdaddress3[1:0] == 2'b00 ? q_ram0 :
				   rdaddress3[1:0] == 2'b01 ? q_ram1 :
				   rdaddress3[1:0] == 2'b10 ? q_ram2 : q_ram3;

	ram16x64	ram16x64_inst_0 (
		.clock (clock),
		.data (data),
		.rdaddress (rdaddress_ram0[7:2]),
		.wraddress (wraddress[7:2]),
		.wren (wren_ram0),
		.q (q_ram0)
	);

	ram16x64	ram16x64_inst_1 (
		.clock (clock),
		.data (data),
		.rdaddress (rdaddress_ram1[7:2]),
		.wraddress (wraddress[7:2]),
		.wren (wren_ram1),
		.q (q_ram1)
	);

	ram16x64	ram16x64_inst_2 (
		.clock (clock),
		.data (data),
		.rdaddress (rdaddress_ram2[7:2]),
		.wraddress (wraddress[7:2]),
		.wren (wren_ram2),
		.q (q_ram2)
	);

	ram16x64	ram16x64_inst_3 (
		.clock (clock),
		.data (data),
		.rdaddress (rdaddress_ram3[7:2]),
		.wraddress (wraddress[7:2]),
		.wren (wren_ram3),
		.q (q_ram3)
	);

endmodule