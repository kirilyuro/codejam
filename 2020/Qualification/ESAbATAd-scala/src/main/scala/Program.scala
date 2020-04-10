import scala.collection.mutable
import Globals._

case class Globals(T: Int, B: Int)
object Globals {
  def read(in: IO.Input): Globals =
    (Globals.apply _).tupled(in.read[Int, Int])

  type Query = Int
  type Response = String
  type ExchangeFn = Query => Response
}

class CaseSolution(caseNumber: Int, globals: Globals, in: IO.Input, out: IO.Output)
  extends Solution.CaseSolutionBase(caseNumber, in, out)
    //    with GlobalExecutionContext
{
  import in._, out._, Math._, globals._

  val finalExchangeResponse: Response = "N"
  val maxExchanges: Int = 150
  lazy val result = getResult

  override protected def writeResult(): Unit = {
    writeLine(result.mkString(""))
  }

  private val exchange = defineExchange { query =>
    writeLine(query)
    readString
  } andThen(_.toInt)

  private def getResult: mutable.ArrayBuffer[Query] = {
    val data = repeat(-1, B)
    var samePair: Option[(Int, Int)] = None
    var diffPair: Option[(Int, Int)] = None

    for (_ <- 1 to 15) {
      var queriesUsed = 0

      samePair.foreach { case (a, _) =>
        val newA = exchange(a)
        queriesUsed += 1

        if (data(a - 1) != newA) {
          complement(data, equality = true)
        }
      }

      diffPair.foreach { case (a, _) =>
        val newA = exchange(a)
        queriesUsed += 1

        if (data(a - 1) != newA) {
          complement(data, equality = false)
        }
      }

      if (queriesUsed == 1) {
        exchange(1)
        queriesUsed += 1
      }

      for (_ <- 1.to(10 - queriesUsed, 2)) {
        data.indexWhere(_ == -1) match {
          case -1 => return data
          case i =>
            data(i) = exchange(i + 1)
            data(B - i - 1) = exchange(B - i)

            if (samePair.isEmpty && data(i) == data(B - i - 1))
              samePair = (i + 1, B - i).some
            if (diffPair.isEmpty && data(i) != data(B - i - 1)) diffPair = (i + 1, B - i).some
        }
      }
    }

    data
  }

  def complement(data: mutable.ArrayBuffer[Int], equality: Boolean): Unit =
    0 until (B / 2) foreach { i =>
      if (data(i) != -1 && data(i) == data(B - i - 1) == equality) {
        data(i) = 1 - data(i)
        data(B - i - 1) = 1 - data(B - i - 1)
      }
    }
}

object Solution extends Utils {
  import IO._

  def main(args: Array[String]): Unit = {
    val (in, out) = (createInput(), createOutput())
    val globals = Globals.read(in)

    (1 to globals.T)
      .map { new CaseSolution(_, globals, in, out) }
      .foreach { _.submitResult() }

    in.close()
    out.close()
  }

  def createInput(): Input = new Input(System.in)
  def createOutput(): Output = new Output(System.out)

  abstract class CaseSolutionBase(caseNumber: Int, in: Input, out: Output) extends Utils {
    protected val exchanges = new mutable.ListBuffer[Exchange]

    def submitResult(): Unit = interactWithJudge {
      writeResult()
      in.read[Response]  // Accept the judge's verdict.
    }

    def result: Any

    protected def writeResult(): Unit

    protected def finalExchangeResponse: Response
    protected def maxExchanges: Int

    protected def defineExchange(exchange: ExchangeFn): ExchangeFn =
      query => interactWithJudge {
        exchange(query).tap(exchanges += Exchange(query, _))
      }

    private def interactWithJudge(f: => Response): Response =
      f.tap(response => if (response == finalExchangeResponse) sys.exit())
  }

  case class Exchange(query: Query, response: Response)
}

object IO extends Utils {
  import java.io.{InputStream, OutputStream, Closeable}
  import reflect._

  private val InputBufferSize: Int = 1024 * 1024 * 5 // 5MiB

  object Format {
    val Int = 'i'
    val Long = 'l'
    val Double = 'd'
    val String = 's'
    val BigInt = 'I'
  }

  object Type {
    lazy val Int: ClassTag[_] = classTag[Int]
    lazy val Long: ClassTag[_] = classTag[Long]
    lazy val Double: ClassTag[_] = classTag[Double]
    lazy val String: ClassTag[_] = classTag[String]
    lazy val BigInt: ClassTag[_] = classTag[BigInt]
  }

  def getTypeFormats(types: ClassTag[_]*): String = {
    types.map {
      case Type.Int => Format.Int
      case Type.Long => Format.Long
      case Type.Double => Format.Double
      case Type.String => Format.String
      case Type.BigInt => Format.BigInt
      case t => throw new IllegalArgumentException(s"Unsupported IO type: $t")
    }.mkString("")
  }

  class Input(stream: InputStream) extends Closeable {
    import scala.io.Source

    private val source = Source.createBufferedSource(stream, InputBufferSize)
    private val lines = source.getLines

    def readInt: Int = read[Int]
    def readLong: Long = read[Long]
    def readDouble: Double = read[Double]
    def readString: String = read[String]
    def readBigInt: BigInt = read[BigInt]

    def read(types: String): List[Any] =
      lines.next.split(' ').toList.zipWithIndex
        .map {
          case (str, index) =>
            val typ = if (index < types.length) types.charAt(index) else types.head
            (str, typ)
        }
        .map {
          case (str, Format.Int) => str.toInt
          case (str, Format.Long) => str.toLong
          case (str, Format.Double) => str.toDouble
          case (str, Format.String) => str
          case (str, Format.BigInt) => BigInt(str)
          case token => throw new UnsupportedOperationException(s"Failed reading input: $token")
        }

    def readMany[T: ClassTag]: List[T] =
      read(getTypeFormats(classTag[T]))
        .map(_.asInstanceOf[T])

    def read[T: ClassTag]: T =
      readMany[T].head

    def read[T1: ClassTag, T2: ClassTag]: (T1, T2) = {
      val objs = read(getTypeFormats(classTag[T1], classTag[T2]))
      (objs.head.asInstanceOf[T1], objs.last.asInstanceOf[T2])
    }

    def read[T1: ClassTag, T2: ClassTag, T3: ClassTag]: (T1, T2, T3) = {
      val objs = read(getTypeFormats(classTag[T1], classTag[T2], classTag[T3]))
      (objs.head.asInstanceOf[T1], objs(1).asInstanceOf[T2], objs.last.asInstanceOf[T3])
    }

    def read[T1: ClassTag, T2: ClassTag, T3: ClassTag, T4: ClassTag]: (T1, T2, T3, T4) = {
      val objs = read(getTypeFormats(classTag[T1], classTag[T2], classTag[T3], classTag[T4]))
      (objs.head.asInstanceOf[T1], objs(1).asInstanceOf[T2], objs(2).asInstanceOf[T3], objs.last.asInstanceOf[T4])
    }

    override def close(): Unit = source.close()
  }

  class Output(stream: OutputStream) extends Closeable {
    import java.io.{BufferedWriter, OutputStreamWriter}

    private val writer = new BufferedWriter(new OutputStreamWriter(stream))

    def writeExplicit(types: String, values: Any*): Unit = {
      val stringValues = values.zipWithIndex.map { case (value, index) =>
        val format = types.charAt(index) match {
          case Format.Int | Format.Long | Format.BigInt => "%d"
          case Format.Double => "%e"
          case Format.String => "%s"
          case t => throw new IllegalArgumentException(s"Unsupported IO type: $t")
        }
        String.format(format, value.asInstanceOf[Object])
      }
      writer.write(stringValues.mkString(" "))
      writer.flush()
    }

    def write[T: ClassTag](value: T): Unit =
      writeExplicit(getTypeFormats(classTag[T]), value)

    def write[T1: ClassTag, T2: ClassTag](v1: T1, v2: T2): Unit =
      writeExplicit(getTypeFormats(classTag[T1], classTag[T2]), v1, v2)

    def write[T1: ClassTag, T2: ClassTag, T3: ClassTag](v1: T1, v2: T2, v3: T3): Unit =
      writeExplicit(getTypeFormats(classTag[T1], classTag[T2], classTag[T3]), v1, v2, v3)

    def write[T1: ClassTag, T2: ClassTag, T3: ClassTag, T4: ClassTag](v1: T1, v2: T2, v3: T3, v4: T4): Unit =
      writeExplicit(getTypeFormats(classTag[T1], classTag[T2], classTag[T3], classTag[T4]), v1, v2, v3, v4)

    def writeLineExplicit(types: String, values: Any*): Unit = {
      writeExplicit(types, values:_*)
      writer.newLine()
      writer.flush()
    }

    def writeLine[T: ClassTag](value: T): Unit =
      writeLineExplicit(getTypeFormats(classTag[T]), value)

    def writeLine[T1: ClassTag, T2: ClassTag](v1: T1, v2: T2): Unit =
      writeLineExplicit(getTypeFormats(classTag[T1], classTag[T2]), v1, v2)

    def writeLine[T1: ClassTag, T2: ClassTag, T3: ClassTag](v1: T1, v2: T2, v3: T3): Unit =
      writeLineExplicit(getTypeFormats(classTag[T1], classTag[T2], classTag[T3]), v1, v2, v3)

    def writeLine[T1: ClassTag, T2: ClassTag, T3: ClassTag, T4: ClassTag](v1: T1, v2: T2, v3: T3, v4: T4): Unit =
      writeLineExplicit(getTypeFormats(classTag[T1], classTag[T2], classTag[T3], classTag[T4]), v1, v2, v3, v4)

    def writeCaseExplicit(caseNumber: Int, types: String, values: Any*): Unit = {
      writer.write(s"Case \u0023$caseNumber:")
      if (values.nonEmpty) writer.write(" ")
      writeLineExplicit(types, values:_*)
    }

    def writeCase(caseNumber: Int): Unit =
      writeCaseExplicit(caseNumber, "")

    def writeCase[T: ClassTag](caseNumber: Int, value: T): Unit =
      writeCaseExplicit(caseNumber, getTypeFormats(classTag[T]), value)

    def writeCase[T1: ClassTag, T2: ClassTag](caseNumber: Int, v1: T1, v2: T2): Unit =
      writeCaseExplicit(caseNumber, getTypeFormats(classTag[T1], classTag[T2]), v1, v2)

    def writeCase[T1: ClassTag, T2: ClassTag, T3: ClassTag](caseNumber: Int, v1: T1, v2: T2, v3: T3): Unit =
      writeCaseExplicit(caseNumber, getTypeFormats(classTag[T1], classTag[T2], classTag[T3]), v1, v2, v3)

    def writeCase[T1: ClassTag, T2: ClassTag, T3: ClassTag, T4: ClassTag](caseNumber: Int, v1: T1, v2: T2, v3: T3, v4: T4): Unit =
      writeCaseExplicit(caseNumber, getTypeFormats(classTag[T1], classTag[T2], classTag[T3], classTag[T4]), v1, v2, v3, v4)

    override def close(): Unit = {
      writer.flush()
      writer.close()
    }
  }
}

trait Utils {
  implicit class RichDouble(value: Double) {
    import Math.{abs, floor}

    def isWhole: Boolean = abs(floor(value) - value) < Double.MinPositiveValue
  }

  implicit class RichGeneric[T](value: T) {
    def tap(f: T => Unit): T = { f(value); value }
    def some: Option[T] = Some(value)
  }

  def memoize[A, B](f: A => B): A => B =
    new collection.mutable.HashMap[A, B]() {
      override def apply(a: A): B = getOrElseUpdate(a, f(a))
    }

  def repeat[A](elem: A, times: Int): mutable.ArrayBuffer[A] = mutable.ArrayBuffer.fill(times)(elem)
  def repeat2D[A](elem: A, rows: Int, columns: Int): mutable.ArrayBuffer[mutable.ArrayBuffer[A]] =
    mutable.ArrayBuffer.fill(rows, columns)(elem)
}

trait GlobalExecutionContext {
  implicit val ec: concurrent.ExecutionContext = concurrent.ExecutionContext.global
}