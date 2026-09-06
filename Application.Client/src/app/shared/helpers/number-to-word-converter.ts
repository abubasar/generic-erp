export class NumberToWordsConverter {
  private units: string[] = [
    "Zero",
    "One",
    "Two",
    "Three",
    "Four",
    "Five",
    "Six",
    "Seven",
    "Eight",
    "Nine",
    "Ten",
    "Eleven",
    "Twelve",
    "Thirteen",
    "Fourteen",
    "Fifteen",
    "Sixteen",
    "Seventeen",
    "Eighteen",
    "Nineteen",
  ];
  private tens: string[] = [
    "",
    "",
    "Twenty",
    "Thirty",
    "Forty",
    "Fifty",
    "Sixty",
    "Seventy",
    "Eighty",
    "Ninety",
  ];

  public convertAmount(amount: number): string {
    try {
      const amount_int: bigint = BigInt(Math.floor(amount));
      const amount_dec: bigint = BigInt(
        Math.round((amount - Number(amount_int)) * 100)
      );

      if (amount_dec === 0n) {
        return this.convert(amount_int) + " Only.";
      } else {
        return (
          this.convert(amount_int) +
          " Point " +
          this.convert(amount_dec) +
          " Only."
        );
      }
    } catch (e) {
      // TODO: handle exception
    }
    return "";
  }

  private convert(i: bigint): string {
    if (i < 20n) {
      return this.units[Number(i)];
    }
    if (i < 100n) {
      return (
        this.tens[Number(i / 10n)] +
        (i % 10n > 0n ? " " + this.convert(i % 10n) : "")
      );
    }
    if (i < 1000n) {
      return (
        this.units[Number(i / 100n)] +
        " Hundred" +
        (i % 100n > 0n ? " And " + this.convert(i % 100n) : "")
      );
    }
    if (i < 100000n) {
      return (
        this.convert(i / 1000n) +
        " Thousand " +
        (i % 1000n > 0n ? " " + this.convert(i % 1000n) : "")
      );
    }
    if (i < 10000000n) {
      return (
        this.convert(i / 100000n) +
        " Lakh " +
        (i % 100000n > 0n ? " " + this.convert(i % 100000n) : "")
      );
    }
    if (i < 1000000000n) {
      return (
        this.convert(i / 10000000n) +
        " Crore " +
        (i % 10000000n > 0n ? " " + this.convert(i % 10000000n) : "")
      );
    }
    return (
      this.convert(i / 1000000000n) +
      " Arab " +
      (i % 1000000000n > 0n ? " " + this.convert(i % 1000000000n) : "")
    );
  }
}


