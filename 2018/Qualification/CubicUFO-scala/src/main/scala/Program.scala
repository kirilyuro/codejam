class CaseSolution(caseNumber: Int, in: IO.Input, out: IO.Output)
  extends Solution.CaseSolutionBase(caseNumber: Int, in: IO.Input, out: IO.Output)
//    with GlobalExecutionContext
{
  import in._, out._, Math._

  val a = readDouble

  override def writeResult(): Unit = {
    writeCase(caseNumber)

    result.foreach(p => writeLine(p.x, p.y, p.z))
  }

  lazy val result: List[Point3D] = {
    val cube = new Cube3D

    if (a < sqrt(2)) {
      findCorrectRotation(cube, cube.rotateX)
    } else {
      cube.rotateX(PI / 4)
      findCorrectRotation(cube, cube.rotateZ)
    }

    computeResult(cube)
  }

  def computeResult(cube: Cube3D): List[Point3D] = {
    val v = cube.vertices

    val p1 = new Point3D(
      x = (v(7).x + v(4).x + v(5).x + v(1).x) / 4,
      y = (v(7).y + v(4).y + v(5).y + v(1).y) / 4,
      z = (v(7).z + v(4).z + v(5).z + v(1).z) / 4
    )

    val p2 = new Point3D(
      x = (v(7).x + v(4).x + v(2).x + v(6).x) / 4,
      y = (v(7).y + v(4).y + v(2).y + v(6).y) / 4,
      z = (v(7).z + v(4).z + v(2).z + v(6).z) / 4
    )

    val p3 = new Point3D(
      x = (v(7).x + v(6).x + v(5).x + v(3).x) / 4,
      y = (v(7).y + v(6).y + v(5).y + v(3).y) / 4,
      z = (v(7).z + v(6).z + v(5).z + v(3).z) / 4
    )

    List(p1, p2, p3)
  }

  def findCorrectRotation(cube: Cube3D, rotationFunc: Double => Unit): Unit = {
    var currRotation = PI / 8
    var cubeArea = cube.shadowArea

    while (abs(cubeArea - a) > 0.0000001) {
      if (cubeArea < a) rotationFunc(currRotation)
      else rotationFunc(-currRotation)

      currRotation /= 2
      cubeArea = cube.shadowArea
    }
  }

  class Point3D(var x: Double, var y: Double, var z: Double)

  class Cube3D() {
    var vertices = List(
      new Point3D(-.5, -.5, -.5),
      new Point3D(-.5, -.5, 0.5),
      new Point3D(-.5, 0.5, -.5),
      new Point3D(0.5, -.5, -.5),
      new Point3D(-.5, 0.5, 0.5),
      new Point3D(0.5, -.5, 0.5),
      new Point3D(0.5, 0.5, -.5),
      new Point3D(0.5, 0.5, 0.5)
    )

    def rotateX(angle: Double): Unit =
      vertices.foreach { v =>
        val ry = v.y * cos(angle) - v.z * sin(angle)
        val rz = v.y * sin(angle) + v.z * cos(angle)

        v.y = ry
        v.z = rz
      }

    def rotateZ(angle: Double): Unit =
      vertices.foreach { v =>
        val rx = v.x * cos(angle) - v.y * sin(angle)
        val ry = v.x * sin(angle) + v.y * cos(angle)

        v.x = rx
        v.y = ry
      }

    def shadowArea: Double = {
      val s1 = abs((vertices(1).x - vertices(4).x) * (vertices(7).z - vertices(4).z) - (vertices(1).z - vertices(4).z) * (vertices(7).x - vertices(4).x))
      val s2 = abs((vertices(4).x - vertices(2).x) * (vertices(0).z - vertices(2).z) - (vertices(4).z - vertices(2).z) * (vertices(0).x - vertices(2).x))
      val s3 = abs((vertices(1).x - vertices(0).x) * (vertices(3).z - vertices(0).z) - (vertices(1).z - vertices(0).z) * (vertices(3).x - vertices(0).x))

      s1 + s2 + s3
    }
  }
}

object Solution extends Utils {
  import java.io.FileInputStream
  import IO._

  lazy val UseStdin: Boolean = !sys.env.get("CJ_USE_FILE_INPUT").exists(_.toBoolean)

  def main(args: Array[String]): Unit = {
    val (in, out) = (createInput(), createOutput())

    (1 to in.readInt)
      .map { new CaseSolution(_, in, out) }
      .tap { _.par.foreach { _.result }}
      .foreach { _.writeResult() }

    in.close()
    out.close()
  }

  def createInput(fileName: String = "./src/main/resources/input.txt"): Input =
    new Input(
      if (UseStdin) System.in
      else new FileInputStream(fileName)
    )

  def createOutput(): Output =
    new Output(System.out)

  abstract class CaseSolutionBase(caseNumber: Int, in: Input, out: Output) extends Utils {
    def writeResult(): Unit
    def result: Any
  }
}

object IO {
  import java.io.{InputStream, OutputStream, Closeable}
  import reflect._

  val InputBufferSize: Int = 1024 * 1024 * 5 // 5MiB

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

    override def close(): Unit = writer.close()
  }
}

trait Utils {
  implicit class RichDouble(value: Double) {
    import Math.{abs, floor}

    def isWhole: Boolean = abs(floor(value) - value) < Double.MinPositiveValue
  }

  implicit class TapOps[T](value: T) {
    def tap(f: T => Unit): T = { f(value); value }
  }

  def memoize[A, B](f: A => B): A => B =
    new collection.mutable.HashMap[A, B]() {
      override def apply(a: A): B = getOrElseUpdate(a, f(a))
    }
}

trait GlobalExecutionContext {
  implicit val ec: concurrent.ExecutionContext = concurrent.ExecutionContext.global
}
